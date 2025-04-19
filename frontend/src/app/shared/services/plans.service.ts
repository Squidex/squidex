/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Versioned, VersionOrTag } from '@app/framework';
import { ChangePlanDto, PlanChangedDto, PlansDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class PlansService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getPlans(appName: string): Observable<Versioned<PlansDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plans`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return PlansDto.fromJSON(body);
            }),
            pretifyError('i18n:plans.loadFailed'));
    }

    public putPlan(appName: string, dto: ChangePlanDto, version: VersionOrTag): Observable<Versioned<PlanChangedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/plan`);

        return HTTP.putVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return PlanChangedDto.fromJSON(body);
            }),
            pretifyError('i18n:plans.changeFailed'));
    }
}