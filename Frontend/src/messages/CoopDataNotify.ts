/* eslint-disable */
import type { CoopChapter } from "./CoopChapter";

/**
 * CmdId: 1979
 * EnetChannelId: 0
 * EnetIsReliable: true
 */
export interface CoopDataNotify {
  ChapterList?: CoopChapter[];
  ViewedChapterList?: number[];
  IsHaveProgress?: boolean;
  CurCoopPoint?: number;
}