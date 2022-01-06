/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export class ContributorDto {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;
    public readonly canRevoke: boolean;

    public get token() {
        return `subject:${this.contributorId}`;
    }

    constructor(
        links: ResourceLinks,
        public readonly contributorId: string,
        public readonly contributorName: string,
        public readonly contributorEmail: string,
        public readonly role: string,
    ) {
        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
        this.canRevoke = hasAnyLink(links, 'delete');
    }
}

export type ContributorsDto =
    Versioned<ContributorsPayload>;

export type ContributorsPayload =
    Readonly<{ items: ReadonlyArray<ContributorDto>; maxContributors: number; canCreate: boolean } & Resource>;

export type AssignContributorDto =
    Readonly<{ contributorId: string; role: string; invite?: boolean }>;

@Injectable()
export class ContributorsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getContributors(appName: string): Observable<ContributorsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/contributors`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseContributors(body);
            }),
            pretifyError('i18n:contributors.loadFailed'));
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
            pretifyError('i18n:contributors.addFailed'));
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
            pretifyError('i18n:contributors.deleteFailed'));
    }
}

function parseContributors(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new ContributorDto(item._links,
            item.contributorId,
            item.contributorName,
            item.contributorEmail,
            item.role));

    const { maxContributors, _links, _meta } = response;

    return { items, maxContributors, _links, _meta, canCreate: hasAnyLink(_links, 'create') };
}
