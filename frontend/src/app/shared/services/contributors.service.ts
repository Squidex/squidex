/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { AssignContributorDto, ContributorsDto } from './../model';

@Service()
export class ContributorsService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getContributors(appName: string): Observable<Versioned<ContributorsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.loadFailed'));
    }

    public postContributor(appName: string, dto: AssignContributorDto, version: VersionOrTag): Observable<Versioned<ContributorsDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto.toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.addFailed'));
    }

    public deleteContributor(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<ContributorsDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return ContributorsDto.fromJSON(body);
            }),
            pretifyError('i18n:contributors.deleteFailed'));
    }
}