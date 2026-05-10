/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { IdentityUser } from "./identity-user";
import { Subscription } from "./subscription";
import { EmailVerification } from "./email-verification";
import { BannedUser } from "./banned-user";

export class User implements IdentityUser {
    name: string;
    isConfirmed: boolean;
    bio: string;
    telegramProfileImage: string;
    telegramId: string;
    image: string;
    isActive: boolean;
    createdAt: Date;
    updatedAt: Date;
    subscriptions: Subscription[] = [];
    emailVerifications: EmailVerification[] = [];
    bannedUser: BannedUser;
}
