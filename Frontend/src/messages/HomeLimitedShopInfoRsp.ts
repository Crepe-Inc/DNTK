/* eslint-disable */
import type { HomeLimitedShopInfo } from "./HomeLimitedShopInfo";

/**
 * CmdId: 4796
 * EnetChannelId: 0
 * EnetIsReliable: true
 */
export interface HomeLimitedShopInfoRsp {
  ShopInfo?: HomeLimitedShopInfo;
  Retcode?: number;
}