/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { ChangesContainer } from "./changes-container";
import { ExternalIdsPerson } from "./external-ids-person";
import { PersonGender } from "./person-gender";
import { ProfileImages } from "./profile-images";
import { MovieCredits } from "./movie-credits";
import { SearchContainer } from "./search-container";
import { TaggedImage } from "./tagged-image";
import { TvCredits } from "./tv-credits";
import { TranslationsContainer } from "./translations-container";

export class Person {
    adult: boolean;
    alsoKnownAs: string[];
    biography: string;
    birthday: Date;
    changes: ChangesContainer;
    deathday: Date;
    externalIds: ExternalIdsPerson;
    gender: PersonGender;
    homepage: string;
    id: number;
    images: ProfileImages;
    imdbId: string;
    movieCredits: MovieCredits;
    name: string;
    placeOfBirth: string;
    popularity: number;
    knownForDepartment: string;
    profilePath: string;
    taggedImages: SearchContainer<TaggedImage>;
    tvCredits: TvCredits;
    translations: TranslationsContainer;
}
