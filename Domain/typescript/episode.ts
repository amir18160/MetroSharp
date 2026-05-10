/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { Season } from "./season";
import { Document } from "./document";

export class Episode {
    id: string;
    episodeNumber: number;
    seasonId: string;
    season: Season;
    documents: Document[] = [];
}
