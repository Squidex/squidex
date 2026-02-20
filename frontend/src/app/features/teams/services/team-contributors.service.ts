/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, AssignContributorDto, ContributorsDto, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/shared';

@Injectable()
export class TeamContributorsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getContributors(teamId: string): Observable<Versioned<ContributorsDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/contributors`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.loadFailed'));
    }

    public postContributor(teamId: string, dto: AssignContributorDto, version: VersionOrTag): Observable<Versioned<ContributorsDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/contributors`);

        return HTTP.postVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.addFailed'));
    }

    public deleteContributor(teamId: string, resource: Resource, version: VersionOrTag): Observable<Versioned<ContributorsDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.deleteFailed'));
    }
}