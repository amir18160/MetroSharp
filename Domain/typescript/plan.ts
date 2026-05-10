/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { Subscription } from "./subscription";

export class Plan {
    id: string;
    title: string;
    code: string;
    length: number;
    price: number;
    discount: number;
    discountReason: string;
    numOfDownloads: number;
    numOfRequest: number;
    subscriptions: Subscription[] = [];
}
