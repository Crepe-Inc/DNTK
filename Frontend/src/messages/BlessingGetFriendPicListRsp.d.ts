/* eslint-disable */
import type { BlessingFriendPicData } from "./BlessingFriendPicData.d.ts";

/**
 * CmdId: 2056
 * EnetChannelId: 0
 * EnetIsReliable: true
 */
export interface BlessingGetFriendPicListRsp {
  Retcode?: number;
  FriendPicDataList?: BlessingFriendPicData[];
}