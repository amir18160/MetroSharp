/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { SearchMovie } from "./search-movie";
import { FindPerson } from "./find-person";
import { SearchTvEpisode } from "./search-tv-episode";
import { SearchTv } from "./search-tv";
import { FindTvSeason } from "./find-tv-season";

export class FindContainer {
    movieResults: SearchMovie[];
    personResults: FindPerson[];
    tvEpisode: SearchTvEpisode[];
    tvResults: SearchTv[];
    tvSeason: FindTvSeason[];
}
