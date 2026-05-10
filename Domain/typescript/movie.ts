/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { AccountState } from "./account-state";
import { SearchCollection } from "./search-collection";
import { ChangesContainer } from "./changes-container";
import { Credits } from "./credits";
import { Genre } from "./genre";
import { Images } from "./images";
import { KeywordsContainer } from "./keywords-container";
import { SearchContainer } from "./search-container";
import { ListResult } from "./list-result";
import { ProductionCompany } from "./production-company";
import { ProductionCountry } from "./production-country";
import { ResultContainer } from "./result-container";
import { ReleaseDatesContainer } from "./release-dates-container";
import { ExternalIdsMovie } from "./external-ids-movie";
import { Releases } from "./releases";
import { ReviewBase } from "./review-base";
import { SearchMovie } from "./search-movie";
import { SpokenLanguage } from "./spoken-language";
import { TranslationsContainer } from "./translations-container";
import { Video } from "./video";
import { SingleResultContainer } from "./single-result-container";
import { WatchProviders } from "./watch-providers";

export class Movie {
    accountStates: AccountState;
    adult: boolean;
    backdropPath: string;
    belongsToCollection: SearchCollection;
    budget: number;
    changes: ChangesContainer;
    credits: Credits;
    genres: Genre[];
    homepage: string;
    id: number;
    images: Images;
    imdbId: string;
    keywords: KeywordsContainer;
    lists: SearchContainer<ListResult>;
    originalLanguage: string;
    originalTitle: string;
    overview: string;
    popularity: number;
    posterPath: string;
    productionCompanies: ProductionCompany[];
    productionCountries: ProductionCountry[];
    releaseDate: Date;
    releaseDates: ResultContainer<ReleaseDatesContainer>;
    externalIds: ExternalIdsMovie;
    releases: Releases;
    revenue: number;
    reviews: SearchContainer<ReviewBase>;
    runtime: number;
    similar: SearchContainer<SearchMovie>;
    recommendations: SearchContainer<SearchMovie>;
    spokenLanguages: SpokenLanguage[];
    status: string;
    tagline: string;
    title: string;
    translations: TranslationsContainer;
    video: boolean;
    videos: ResultContainer<Video>;
    watchProviders: SingleResultContainer<{ [key: string]: WatchProviders; }>;
    voteAverage: number;
    voteCount: number;
}
