/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    ApiUrlConfig,
    HTTP,
    Version
} from 'framework';

export class AppPlansDto {
    constructor(
        public readonly currentPlanId: string,
        public readonly planOwner: string,
        public readonly hasPortal: boolean,
        public readonly hasConfigured: boolean,
        public readonly plans: PlanDto[]
    ) {
    }
}

export class PlanDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly costs: string,
        public readonly maxApiCalls: number,
        public readonly maxAssetSize: number,
        public readonly maxContributors: number
    ) {
    }
}

export class PlanChangedDto {
    constructor(
        public readonly redirectUri: string
    ) {
    }
}

export class ChangePlanDto {
    constructor(
        public readonly planId: string
    ) {
    }
}

@Injectable()
export class PlansService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPlans(appName: string, version?: Version): Observable<AppPlansDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plans`);

        return HTTP.getVersioned(this.http, url, version)
                .map(response => {
                    const items: any[] = response.plans;

                    return new AppPlansDto(
                        response.currentPlanId,
                        response.planOwner,
                        response.hasPortal,
                        response.hasConfigured,
                        items.map(item => {
                            return new PlanDto(
                                item.id,
                                item.name,
                                item.costs,
                                item.maxApiCalls,
                                item.maxAssetSize,
                                item.maxContributors);
                        }));
                })
                .pretifyError('Failed to load plans. Please reload.');
    }

    public putPlan(appName: string, dto: ChangePlanDto, version?: Version): Observable<PlanChangedDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plan`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .map(response => {
                    return new PlanChangedDto(response.redirectUri);
                })
                .pretifyError('Failed to change plan. Please reload.');
    }
}