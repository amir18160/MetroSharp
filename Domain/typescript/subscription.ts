/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { User } from "./user";
import { Plan } from "./plan";
import { SubscriptionStatus } from "./subscription-status";

export class Subscription {
    id: string;
    userId: string;
    user: User;
    planId: string;
    plan: Plan;
    startedAt: Date;
    endAt: Date;
    paidPrice: number;
    status: SubscriptionStatus;
}
