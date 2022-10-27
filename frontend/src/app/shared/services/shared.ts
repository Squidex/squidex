/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { hasAnyLink, Resource, ResourceLinks, Versioned } from '@app/framework';

export class ContributorDto {
    public readonly _links: ResourceLinks;

    public readonly canRevoke: boolean;
    public readonly canUpdate: boolean;

    public get token() {
        return `subject:${this.contributorId}`;
    }

    constructor(links: ResourceLinks,
        public readonly contributorId: string,
        public readonly contributorName: string,
        public readonly contributorEmail: string,
        public readonly role: string,
    ) {
        this._links = links;

        this.canRevoke = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export type ContributorsDto = Versioned<ContributorsPayload>;

export type ContributorsPayload = Readonly<{
    // The list of contributors.
    items: ReadonlyArray<ContributorDto>;

    // The number of allowed contributors.
    maxContributors: number;

    // True, if the user has been invited.
    isInvited?: boolean;

    // True, if the user has permission to create a contributor.
    canCreate?: boolean;
}>;

export type AssignContributorDto = Readonly<{
    // The user ID.
    contributorId: string;

    // The role for the contributor.
    role: string;

    // True, if the user should be invited.
    invite?: boolean;
}>;

export class PlanDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly costs: string,
        public readonly confirmText: string | undefined,
        public readonly yearlyId: string,
        public readonly yearlyCosts: string,
        public readonly yearlyConfirmText: string | undefined,
        public readonly maxApiBytes: number,
        public readonly maxApiCalls: number,
        public readonly maxAssetSize: number,
        public readonly maxContributors: number,
    ) {
    }
}

export type PlanLockedReason =  'None' | 'NotOwner' |  'NoPermission' | 'ManagedByTeam';

export type PlansDto = Versioned<PlansPayload>;

export type PlansPayload = Readonly<{
    // The ID of the current plan.
    currentPlanId: string;

    // The user, who owns the plan.
    planOwner: string;

    // The actual plans.
    plans: ReadonlyArray<PlanDto>;

    // The portal link if available.
    portalLink?: string;

    // The referral code.
    referral?: ReferralDto;

    // The reason why the plan cannot be changed.
    locked: PlanLockedReason;
}>;

export type ReferralDto = Readonly<{
    // The referral code.
    code: string;

    // The amount earned.
    earned: string;

    // The referral condition.
    condition: string;
}>;

export type PlanChangedDto = Readonly<{
    // The redirect URI.
    redirectUri?: string;
}>;

export type ChangePlanDto = Readonly<{
    // The new plan ID.
    planId: string;
}>;

export function parsePlans(response: { plans: any[] } & any): PlansPayload {
    const { plans: list, ...more } = response;
    const plans = list.map(parsePlan);

    return { ...more, plans };
}

export function parsePlan(response: any) {
    return new PlanDto(
        response.id,
        response.name,
        response.costs,
        response.confirmText,
        response.yearlyId,
        response.yearlyCosts,
        response.yearlyConfirmText,
        response.maxApiBytes,
        response.maxApiCalls,
        response.maxAssetSize,
        response.maxContributors);
}

export function parseContributors(response: { items: any[]; maxContributors: number } & Resource): ContributorsPayload {
    const { items: list, maxContributors, _meta, _links } = response;
    const items = list.map(parseContributor);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, maxContributors, canCreate, isInvited: _meta?.['isInvited'] === '1' };
}

export function parseContributor(response: any) {
    return new ContributorDto(response._links,
        response.contributorId,
        response.contributorName,
        response.contributorEmail,
        response.role);
}