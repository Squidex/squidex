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
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export type ContributorsDto = Versioned<{
    readonly contributors: ContributorDto[],
    readonly maxContributors: number
}>;

export class ContributorDto extends Model<AssignContributorDto> {
    constructor(
        public readonly contributorId: string,
        public readonly role: string
    ) {
        super();
    }
}

export interface ContributorAssignedDto {
    readonly contributorId: string;
    readonly isCreated?: boolean;
}

export interface AssignContributorDto  {
    readonly contributorId: string;
    readonly role: string;
    readonly invite?: boolean;
}

@Injectable()
export class ContributorsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getContributors(appName: string): Observable<ContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                mapVersioned(payload => {
                    const body = payload.body;

                    const items: any[] = body.contributors;

                    const contributors =
                        items.map(item =>
                            new ContributorDto(
                                item.contributorId,
                                item.role));

                    return { contributors, maxContributors: body.maxContributors };
                }),
                pretifyError('Failed to load contributors. Please reload.'));
    }

    public postContributor(appName: string, dto: AssignContributorDto, version: Version): Observable<Versioned<ContributorAssignedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
                mapVersioned(payload => {
                    return <ContributorAssignedDto>payload.body;
                }),
                tap(() => {
                    this.analytics.trackEvent('Contributor', 'Configured', appName);
                }),
                pretifyError('Failed to add contributors. Please reload.'));
    }

    public deleteContributor(appName: string, contributorId: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors/${contributorId}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Contributor', 'Deleted', appName);
                }),
                pretifyError('Failed to delete contributors. Please reload.'));
    }
}