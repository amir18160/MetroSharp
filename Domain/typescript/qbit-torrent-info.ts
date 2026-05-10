/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TimeSpan } from "./time-span";
import { QbitTorrentState } from "./qbit-torrent-state";

export class QbitTorrentInfo {
    hash: string;
    name: string;
    magnetUri: string;
    size: number;
    progress: number;
    downloadSpeed: number;
    uploadSpeed: number;
    priority: number;
    connectedSeeds: number;
    totalSeeds: number;
    connectedLeechers: number;
    totalLeechers: number;
    ratio: number;
    estimatedTime: TimeSpan;
    state: QbitTorrentState;
    sequentialDownload: boolean;
    firstLastPiecePrioritized: boolean;
    category: string;
    tags: string[];
    superSeeding: boolean;
    forceStart: boolean;
    savePath: string;
    addedOn: Date;
    completionOn: Date;
    currentTracker: string;
    downloadLimit: number;
    uploadLimit: number;
    downloaded: number;
    uploaded: number;
    downloadedInSession: number;
    uploadedInSession: number;
    incompletedSize: number;
    completedSize: number;
    ratioLimit: number;
    lastSeenComplete: Date;
    lastActivityTime: Date;
    activeTime: TimeSpan;
    automaticTorrentManagement: boolean;
    totalSize: number;
    seedingTime: TimeSpan;
    contentPath: string;
}
