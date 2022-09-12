/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, ChangePlanDto, HTTP, mapVersioned, parsePlans, PlanChangedDto, PlansDto, pretifyError, Version, Versioned } from '@app/shared';

@Injectable()
export class TeamPlansService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getPlans(teamId: string): Observable<PlansDto> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/plans`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parsePlans(body);
            }),
            pretifyError('i18n:plans.loadFailed'));
    }

    public putPlan(teamId: string, dto: ChangePlanDto, version: Version): Observable<Versioned<PlanChangedDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/plan`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return <PlanChangedDto>body;
            }),
            pretifyError('i18n:plans.changeFailed'));
    }
}