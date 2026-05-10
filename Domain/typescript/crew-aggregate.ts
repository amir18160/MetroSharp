/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { CrewBase } from "./crew-base";
import { CrewJob } from "./crew-job";

export class CrewAggregate implements CrewBase {
    jobs: CrewJob[];
    totalEpisodeCount: number;
}
