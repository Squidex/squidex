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
import { ChangePlanDto, parsePlans, PlanChangedDto, PlansDto } from './shared';
export * from './shared';

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