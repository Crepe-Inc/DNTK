﻿using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Text;
using PacketDotNet;
using Serilog;
using SharpPcap;
using SharpPcap.LibPcap;

namespace DNToolKit.Sniffer;

public class Sniffer
{

    private LibPcapLiveDevice _pcapDevice;
    private UdpHandler _udpHandler;

    private object lockObject = new();

    public void OnPacketArrival(object sender, PacketCapture e)
    {
        //forward it to the pcap dumper as well
        Program.PcapDumper.PcapOnPacketArrival(sender, e);
        
        _udpHandler.HandleRawCapture(e.GetPacket());
    }

    public void Start(bool choose)
    {
        Log.Information("SharpPcap {0}, StartLiveCapture", (object)Pcap.SharpPcapVersion);
        if (choose)
        {
            _pcapDevice = ChoosePcapDevice();
        }
        else
        {
            _pcapDevice = GetPcapDevice();
        }
        SharpPcapCapturer();

        _udpHandler = new UdpHandler();
    }
    
    public void AddPcap(byte[] bytes)
    {
        return;
        //use cli for now please.
        lock (lockObject)
        {
            //i dont like this but the library calls for it...

            string tempFilePathWithFileName = Path.GetTempFileName( );
            File.WriteAllBytes(tempFilePathWithFileName, bytes);
            var pcap = new CaptureFileReaderDevice(tempFilePathWithFileName);

            Log.Information(DateTime.Now.ToString("hh:mm:ss t z"));
            List<RawCapture> bs = new();
        
            pcap.OnPacketArrival += delegate(object sender, PacketCapture capture)
            {
                bs.Add(capture.GetPacket());
            };
            
            pcap.Open();
            pcap.Filter = "udp portrange 22101-22102";

            //this returns when EOF
            pcap.Capture();
            pcap.Close();
            //i guessed this lmfao
            bs.Sort((x, y)=>x.Timeval.Date.Ticks < y.Timeval.Date.Ticks ? -1 : 1);

            var a = new List<string>();

            var count = 0;
            foreach (var rawCapture in bs)
            {
                
                //slay
                a.Add($"{rawCapture.Timeval.Date.Ticks}");

                var udpPacket = PacketDotNet.Packet.ParsePacket(LinkLayers.Ethernet,
                        rawCapture.Data)
                    .Extract<IPv4Packet>()
                    .Extract<UdpPacket>();
                var packetBytes = udpPacket.PayloadData;
                
                
                a.Add($"{Convert.ToHexString(packetBytes)}");

                count++;
                _udpHandler.HandleRawCapture(rawCapture);
            }
            File.Delete(tempFilePathWithFileName);
        }
    }

    public void SharpPcapCapturer()
    {
        

        _pcapDevice.OnPacketArrival += OnPacketArrival;
        //_pcapDevice.OnPacketArrival += Program.CaptureDumper.PcapOnPacketArrival;
        
        const int readTimeout = 1000;
        _pcapDevice.Open(DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, readTimeout);
        _pcapDevice.Filter = "udp portrange 22101-22102";
        _pcapDevice.StartCapture();


        Log.Information("-- Listening on {0} {1}", (object)_pcapDevice.Name, (object)_pcapDevice.Description);



    }
    
    public void Close()
    {
        _pcapDevice.StopCapture();
        Log.Information("-- Capture stopped.");
        Log.Information(_pcapDevice.Statistics.ToString());
        Program.PcapDumper.Close();
        _udpHandler.Close();
        Log.Information("Sniffer stopped...");

    }

    private static LibPcapLiveDevice ChoosePcapDevice()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        Console.WriteLine();
        Console.WriteLine("The following devices are available on this machine:");
        Console.WriteLine("----------------------------------------------------");
        Console.WriteLine();
        int b = 0;
        var interfaces = PcapInterface.GetAllPcapInterfaces();
        foreach (PcapInterface allPcapInterface in interfaces)
        {
            
            Console.WriteLine("{0})\t{1} - {2}",b,allPcapInterface.Name, allPcapInterface.Description);
            b++;
        }
        

        int i = 0;
        Console.WriteLine();
        Console.Write("-- Please choose a device to capture: ");
        i = int.Parse(Console.ReadLine());
        

 

        var device = interfaces[i];
        return new LibPcapLiveDevice(device);
    }

    // taken from devove's proj
    private static LibPcapLiveDevice GetPcapDevice()
    {
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (PcapInterface allPcapInterface in PcapInterface.GetAllPcapInterfaces())
        {
            var friendlyName = allPcapInterface.FriendlyName ?? string.Empty;
            if (friendlyName.ToLower().Contains("loopback") || friendlyName is "any" or "virbr0-nic" ||friendlyName.ToLower().Contains("wsl") ) continue;
            
            var networkInterface = networkInterfaces.FirstOrDefault(ni => ni.Name == friendlyName);
            if ((networkInterface != null ? (networkInterface.OperationalStatus != OperationalStatus.Up ? 1 : 0) : 1) !=
                0) continue;
            
            using (var device = new LibPcapLiveDevice(allPcapInterface))
            {
                LinkLayers linkType;
                try
                {
                    device.Open();
                    linkType = device.LinkType;
                }
                catch (PcapException ex)
                {
                    Console.WriteLine(ex);
                    continue;
                }
                if (linkType == LinkLayers.Ethernet)
                    return device;
            }
        }
        throw new InvalidOperationException("No ethernet pcap supported devices found, are you running as a user with access to adapters (root on Linux)?");
    }
}