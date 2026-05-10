/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TagType } from "./tag-type";
import { OmdbItem } from "./omdb-item";

export class Tag {
    id: string;
    type: TagType;
    description: string;
    isPinned: boolean;
    createdAt: Date;
    omdbItemId: string;
    omdbItem: OmdbItem;
}
