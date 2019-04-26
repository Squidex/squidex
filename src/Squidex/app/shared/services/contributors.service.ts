/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    HTTP,
    Model,
    pretifyError,
    Version,
    Versioned
} from '@app/framework';

export class ContributorsDto extends Model<ContributorsDto> {
    constructor(
        public readonly contributors: ContributorDto[],
        public readonly maxContributors: number,
        public readonly version: Version
    ) {
        super();
    }
}

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
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.contributors;

                    return new ContributorsDto(
                        items.map(item => {
                            return new ContributorDto(
                                item.contributorId,
                                item.role);
                        }),
                        body.maxContributors, response.version);
                }),
                pretifyError('Failed to load contributors. Please reload.'));
    }

    public postContributor(appName: string, dto: AssignContributorDto, version: Version): Observable<Versioned<ContributorAssignedDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
                map(response => {
                    const body: any = response.payload.body;

                    return new Versioned(response.version, body);
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