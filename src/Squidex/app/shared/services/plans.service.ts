/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    HTTP,
    mapVersioned,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export type PlansDto = Versioned<{
    readonly currentPlanId: string,
    readonly planOwner: string,
    readonly hasPortal: boolean,
    readonly plans: PlanDto[]
}>;

export class PlanDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly costs: string,
        public readonly yearlyId: string,
        public readonly yearlyCosts: string,
        public readonly maxApiCalls: number,
        public readonly maxAssetSize: number,
        public readonly maxContributors: number
    ) {
    }
}

export interface PlanChangedDto {
    readonly redirectUri?: string;
}

export interface ChangePlanDto {
    readonly planId: string;
}

@Injectable()
export class PlansService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getPlans(appName: string): Observable<PlansDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plans`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                const items: any[] = body.plans;

                const { hasPortal, currentPlanId, planOwner } = body;

                const plans = {
                    currentPlanId,
                    planOwner,
                    plans: items.map(item =>
                        new PlanDto(
                            item.id,
                            item.name,
                            item.costs,
                            item.yearlyId,
                            item.yearlyCosts,
                            item.maxApiCalls,
                            item.maxAssetSize,
                            item.maxContributors)),
                    hasPortal
                };

                return plans;
            }),
            pretifyError('Failed to load plans. Please reload.'));
    }

    public putPlan(appName: string, dto: ChangePlanDto, version: Version): Observable<Versioned<PlanChangedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plan`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            mapVersioned(payload => {
                return <PlanChangedDto>payload.body;
            }),
            tap(() => {
                this.analytics.trackEvent('Plan', 'Changed', appName);
            }),
            pretifyError('Failed to change plan. Please reload.'));
    }
}