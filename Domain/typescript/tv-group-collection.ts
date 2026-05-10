/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TvGroupType } from "./tv-group-type";
import { NetworkWithLogo } from "./network-with-logo";
import { TvGroup } from "./tv-group";

export class TvGroupCollection {
    id: string;
    name: string;
    type: TvGroupType;
    description: string;
    network: NetworkWithLogo;
    episodeCount: number;
    groupCount: number;
    groups: TvGroup[];
}
