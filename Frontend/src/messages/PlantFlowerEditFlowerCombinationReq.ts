/* eslint-disable */
import type { CustomGadgetTreeInfo } from "./CustomGadgetTreeInfo";

/**
 * CmdId: 8843
 * EnetChannelId: 0
 * EnetIsReliable: true
 * IsAllowClient: true
 */
export interface PlantFlowerEditFlowerCombinationReq {
  FlowerCombinationInfo?: CustomGadgetTreeInfo;
  EntityId?: number;
  ScheduleId?: number;
}