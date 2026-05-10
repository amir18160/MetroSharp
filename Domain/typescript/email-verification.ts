/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { EmailPurpose } from "./email-purpose";
import { User } from "./user";

export class EmailVerification {
    id: string = "a41791ab-5069-4576-bb8f-0e63a27768b8";
    userId: string;
    code: string;
    purpose: EmailPurpose;
    expiration: Date;
    used: boolean;
    user: User;
}
