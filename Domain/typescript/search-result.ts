/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { SearchCategory } from "./search-category";

export class SearchResult {
    id: number;
    guid: string;
    age: number;
    ageHours: number;
    ageMinutes: number;
    size: number;
    files: number;
    grabs: number;
    indexerId: number;
    indexer: string;
    subGroup: string;
    releaseHash: string;
    title: string;
    sortTitle: string;
    imdbId: number;
    tmdbId: number;
    tvdbId: number;
    tvMazeId: number;
    publishDate: Date;
    commentUrl: string;
    downloadUrl: string;
    infoUrl: string;
    posterUrl: string;
    indexerFlags: string[] = [];
    categories: SearchCategory[] = [];
    magnetUrl: string;
    infoHash: string;
    seeders: number;
    leechers: number;
    protocol: string;
    downloadClientId: number;
}
