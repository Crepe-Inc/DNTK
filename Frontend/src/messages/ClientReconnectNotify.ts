/* eslint-disable */
import type { ClientReconnectReason } from "./ClientReconnectReason";

/**
 * CmdId: 75
 * EnetChannelId: 0
 * EnetIsReliable: true
 */
export interface ClientReconnectNotify {
  Reason?: ClientReconnectReason;
}