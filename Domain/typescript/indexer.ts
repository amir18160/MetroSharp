/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { Field } from "./field";
import { IndexerMessage } from "./indexer-message";
import { Capabilities } from "./capabilities";
import { IndexerStatus } from "./indexer-status";

export class Indexer {
    id: number;
    name: string;
    fields: Field[];
    implementationName: string;
    implementation: string;
    configContract: string;
    infoLink: string;
    message: IndexerMessage;
    tags: number[];
    presets: string[];
    indexerUrls: string[];
    legacyUrls: string[];
    definitionName: string;
    description: string;
    language: string;
    encoding: string;
    enable: boolean;
    redirect: boolean;
    supportsRss: boolean;
    supportsSearch: boolean;
    supportsRedirect: boolean;
    supportsPagination: boolean;
    appProfileId: number;
    protocol: string;
    privacy: string;
    capabilities: Capabilities;
    priority: number;
    downloadClientId: number;
    added: Date;
    status: IndexerStatus;
    sortName: string;
}
