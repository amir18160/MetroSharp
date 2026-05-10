/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { SearchPerson } from "./search-person";
import { PersonGender } from "./person-gender";

export class FindPerson implements SearchPerson {
    gender: PersonGender;
    knownForDepartment: string;
}
