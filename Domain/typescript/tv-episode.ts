/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TvEpisodeBase } from "./tv-episode-base";
import { TvAccountState } from "./tv-account-state";
import { CreditsWithGuestStars } from "./credits-with-guest-stars";
import { Crew } from "./crew";
import { ExternalIdsTvEpisode } from "./external-ids-tv-episode";
import { Cast } from "./cast";
import { StillImages } from "./still-images";
import { ResultContainer } from "./result-container";
import { Video } from "./video";
import { TranslationsContainer } from "./translations-container";

export class TvEpisode implements TvEpisodeBase {
    accountStates: TvAccountState;
    credits: CreditsWithGuestStars;
    crew: Crew[];
    externalIds: ExternalIdsTvEpisode;
    guestStars: Cast[];
    images: StillImages;
    videos: ResultContainer<Video>;
    translations: TranslationsContainer;
}
