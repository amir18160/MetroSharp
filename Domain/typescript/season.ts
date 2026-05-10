/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { OmdbItem } from "./omdb-item";
import { Episode } from "./episode";

export class Season {
    id: string;
    seasonNumber: number;
    omdbItemId: string;
    omdbItem: OmdbItem;
    episodes: Episode[] = [];
}
