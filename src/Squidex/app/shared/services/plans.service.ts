/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, Version } from 'framework';
import { AuthService } from './auth.service';

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


export class ChangePlanDto {
    constructor(
        public readonly planId: string
    ) {
    }
}

@Injectable()
export class PlansService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getPlans(appName: string, version?: Version): Observable<AppPlansDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plans`);

        return this.authService.authGet(url, version)
                .map(response => response.json())
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
                .catchError('Failed to load plans. Please reload.');
    }

    public putPlan(appName: string, dto: ChangePlanDto, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plan`);

        return this.authService.authPut(url, dto, version)
                .catchError('Failed to change plan. Please reload.');
    }
}