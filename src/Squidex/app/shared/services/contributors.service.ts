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
    Metadata,
    Model,
    pretifyError,
    Resource,
    ResourceLinks,
    Version,
    Versioned,
    withLinks
} from '@app/framework';

export type ContributorsDto = Versioned<ContributorsPayload>;
export type ContributorsPayload = {
    readonly _links?: ResourceLinks,
    readonly _meta?: Metadata
    readonly contributors: ContributorDto[],
    readonly maxContributors: number
};

export class ContributorDto extends Model<AssignContributorDto> {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly contributorId: string,
        public readonly role: string
    ) {
        super();
    }
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

                    return parseContributors(body);
                }),
                pretifyError('Failed to load contributors. Please reload.'));
    }

    public postContributor(appName: string, dto: AssignContributorDto, version: Version): Observable<ContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
                mapVersioned(payload => {
                    const body = payload.body;

                    return parseContributors(body);
                }),
                tap(() => {
                    this.analytics.trackEvent('Contributor', 'Configured', appName);
                }),
                pretifyError('Failed to add contributors. Please reload.'));
    }

    public deleteContributor(appName: string, resource: Resource, version: Version): Observable<ContributorsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
                mapVersioned(payload => {
                    const body = payload.body;

                    return parseContributors(body);
                }),
                tap(() => {
                    this.analytics.trackEvent('Contributor', 'Deleted', appName);
                }),
                pretifyError('Failed to delete contributors. Please reload.'));
    }
}

function parseContributors(body: any) {
    const items: any[] = body.contributors;

    const contributors =
        items.map(item =>
            withLinks(
                new ContributorDto(
                    item.contributorId,
                    item.role),
                item));

    return withLinks({ contributors, maxContributors: body.maxContributors, _links: {} }, body);
}