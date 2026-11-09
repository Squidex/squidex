/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, ChangePlanDto, HTTP, mapVersioned, PlanChangedDto, PlansDto, pretifyError, Versioned, VersionOrTag } from '@app/shared';

@Service({ autoProvided: false })
export class TeamPlansService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getPlans(teamId: string): Observable<Versioned<PlansDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/plans`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return PlansDto.fromJSON(body);
            }),
            pretifyError('i18n:plans.loadFailed'));
    }

    public putPlan(teamId: string, dto: ChangePlanDto, version: VersionOrTag): Observable<Versioned<PlanChangedDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/plan`);

        return HTTP.putVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return PlanChangedDto.fromJSON(body);
            }),
            pretifyError('i18n:plans.changeFailed'));
    }
}