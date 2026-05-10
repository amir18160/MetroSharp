/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { SearchBase } from "./search-base";
import { KnownForBase } from "./known-for-base";

export class SearchPerson implements SearchBase {
    adult: boolean;
    knownFor: KnownForBase[];
    name: string;
    profilePath: string;
}
