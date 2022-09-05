/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Version, Versioned } from '@app/framework';

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

export type PlansDto = Versioned<PlansPayload>;

export type PlansPayload = Readonly<{
    // The ID of the current plan.
    currentPlanId: string;

    // The user, who owns the plan.
    planOwner: string;

    // True, if the installation has a billing portal.
    hasPortal: boolean;

    // The actual plans.
    plans: ReadonlyArray<PlanDto>;
}>;

export type PlanChangedDto = Readonly<{
    // The redirect URI.
    redirectUri?: string;
}>;

export type ChangePlanDto = Readonly<{
    // The new plan ID.
    planId: string;
}>;

@Injectable()
export class PlansService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getPlans(appName: string): Observable<PlansDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plans`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parsePlans(body);
            }),
            pretifyError('i18n:plans.loadFailed'));
    }

    public putPlan(appName: string, dto: ChangePlanDto, version: Version): Observable<Versioned<PlanChangedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plan`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return <PlanChangedDto>body;
            }),
            pretifyError('i18n:plans.changeFailed'));
    }
}

function parsePlans(response: { plans: any[]; hasPortal: boolean; currentPlanId: string; planOwner: string }): PlansPayload {
    const { plans: list, currentPlanId, hasPortal, planOwner } = response;
    const plans = list.map(parsePlan);

    return { plans, planOwner, currentPlanId, hasPortal };
}

function parsePlan(response: any) {
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