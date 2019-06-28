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
    hasAnyLink,
    HTTP,
    mapVersioned,
    pretifyError,
    Resource,
    ResourceLinks,
    Version,
    Versioned
} from '@app/framework';

export type ContributorsDto = Versioned<ContributorsPayload>;
export type ContributorsPayload = {
    readonly items: ContributorDto[];

    readonly maxContributors: number;

    readonly canCreate: boolean;
} & Resource;

export class ContributorDto {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;
    public readonly canRevoke: boolean;

    constructor(
        links: ResourceLinks,
        public readonly contributorId: string,
        public readonly role: string
    ) {
        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
        this.canRevoke = hasAnyLink(links, 'delete');
    }
}

export interface AssignContributorDto {
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

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseContributors(body);
            }),
            pretifyError('Failed to load contributors. Please reload.'));
    }

    public postContributor(appName: string, dto: AssignContributorDto, version: Version): Observable<ContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
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

function parseContributors(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new ContributorDto(item._links,
            item.contributorId,
            item.role));

    const { maxContributors, _links, _meta } = response;

    return { items, maxContributors, _links, _meta, canCreate: hasAnyLink(_links, 'create') };
}