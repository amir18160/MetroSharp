/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { CreditType } from "./credit-type";
import { CreditMedia } from "./credit-media";
import { MediaType } from "./media-type";
import { CreditPerson } from "./credit-person";

export class Credit {
    creditType: CreditType;
    department: string;
    id: string;
    job: string;
    media: CreditMedia;
    mediaType: MediaType;
    person: CreditPerson;
}
