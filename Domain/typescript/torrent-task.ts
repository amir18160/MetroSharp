/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TorrentTaskState } from "./torrent-task-state";
import { TorrentTaskType } from "./torrent-task-type";
import { TorrentTaskPriority } from "./torrent-task-priority";
import { TaskDownloadProgress } from "./task-download-progress";
import { TaskUploadProgress } from "./task-upload-progress";
import { SubtitleVideoPair } from "./subtitle-video-pair";

export class TorrentTask {
    id: string;
    title: string;
    torrentTitle: string;
    state: TorrentTaskState;
    errorMessage: string;
    torrentHash: string;
    magnet: string;
    fileSavingPath: string;
    taskType: TorrentTaskType;
    priority: TorrentTaskPriority;
    subtitleStoredPath: string;
    subtitleSavingPath: string;
    imdbId: string;
    seasonNumber: number;
    episodeNumber: number;
    regex: string;
    startTime: Date;
    endTime: Date;
    downloadStartTime: Date;
    downloadEndTime: Date;
    userId: string;
    ffmpegPID: number;
    taskDownloadProgress: TaskDownloadProgress;
    taskUploadProgress: TaskUploadProgress[] = [];
    subtitleVideoPairs: SubtitleVideoPair[] = [];
    createdAt: Date;
    updatedAt: Date;
}
