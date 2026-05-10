/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { DocumentType } from "./document-type";
import { Episode } from "./episode";
import { OmdbItem } from "./omdb-item";

export class Document {
    id: string;
    fileName: string;
    mimeType: string;
    fileId: string;
    uniqueFileId: string;
    fileSize: number;
    chatName: string;
    messageId: number;
    chatId: number;
    isSubbed: boolean;
    resolution: string;
    codec: string;
    encoder: string;
    type: DocumentType;
    createdAt: Date;
    updatedAt: Date;
    episodeId: string;
    episode: Episode;
    omdbItemId: string;
    omdbItem: OmdbItem;
}
