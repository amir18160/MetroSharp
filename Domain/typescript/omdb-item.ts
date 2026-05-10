/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { OmdbItemType } from "./omdb-item-type";
import { Season } from "./season";
import { Document } from "./document";
import { Tag } from "./tag";

export class OmdbItem {
    id: string;
    imdbId: string;
    title: string;
    rated: string;
    released: string;
    runtime: string;
    genres: string[];
    actors: string[];
    plot: string;
    plotFa: string;
    languages: string[];
    countries: string[];
    awards: string;
    poster: string;
    metascore: number;
    rottenTomatoesScore: number;
    imdbRating: number;
    imdbVotes: number;
    type: OmdbItemType;
    boxOffice: string;
    totalSeasons: number;
    directors: string[];
    writers: string[];
    year: number;
    seasons: Season[] = [];
    documents: Document[] = [];
    tags: Tag[] = [];
}
