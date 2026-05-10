/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { AccountState } from "./account-state";
import { ResultContainer } from "./result-container";
import { AlternativeTitle } from "./alternative-title";
import { ChangesContainer } from "./changes-container";
import { ContentRating } from "./content-rating";
import { CreatedBy } from "./created-by";
import { Credits } from "./credits";
import { CreditsAggregate } from "./credits-aggregate";
import { TvGroupCollection } from "./tv-group-collection";
import { ExternalIdsTvShow } from "./external-ids-tv-show";
import { Genre } from "./genre";
import { Images } from "./images";
import { Keyword } from "./keyword";
import { TvEpisodeBase } from "./tv-episode-base";
import { NetworkWithLogo } from "./network-with-logo";
import { ProductionCompany } from "./production-company";
import { ProductionCountry } from "./production-country";
import { SearchContainer } from "./search-container";
import { SearchTv } from "./search-tv";
import { ReviewBase } from "./review-base";
import { SearchTvSeason } from "./search-tv-season";
import { SpokenLanguage } from "./spoken-language";
import { TranslationsContainer } from "./translations-container";
import { Video } from "./video";
import { SingleResultContainer } from "./single-result-container";
import { WatchProviders } from "./watch-providers";

export class TvShow {
    adult: boolean;
    accountStates: AccountState;
    alternativeTitles: ResultContainer<AlternativeTitle>;
    backdropPath: string;
    changes: ChangesContainer;
    contentRatings: ResultContainer<ContentRating>;
    createdBy: CreatedBy[];
    credits: Credits;
    aggregateCredits: CreditsAggregate;
    episodeGroups: ResultContainer<TvGroupCollection>;
    episodeRunTime: number[];
    externalIds: ExternalIdsTvShow;
    firstAirDate: Date;
    genreIds: number[];
    genres: Genre[];
    homepage: string;
    id: number;
    images: Images;
    inProduction: boolean;
    keywords: ResultContainer<Keyword>;
    languages: string[];
    lastAirDate: Date;
    lastEpisodeToAir: TvEpisodeBase;
    name: string;
    nextEpisodeToAir: TvEpisodeBase;
    networks: NetworkWithLogo[];
    numberOfEpisodes: number;
    numberOfSeasons: number;
    originalLanguage: string;
    originalName: string;
    originCountry: string[];
    overview: string;
    popularity: number;
    posterPath: string;
    productionCompanies: ProductionCompany[];
    productionCountries: ProductionCountry[];
    recommendations: SearchContainer<SearchTv>;
    reviews: SearchContainer<ReviewBase>;
    seasons: SearchTvSeason[];
    similar: SearchContainer<SearchTv>;
    spokenLanguages: SpokenLanguage[];
    status: string;
    tagline: string;
    translations: TranslationsContainer;
    type: string;
    videos: ResultContainer<Video>;
    watchProviders: SingleResultContainer<{ [key: string]: WatchProviders; }>;
    voteAverage: number;
    voteCount: number;
}
