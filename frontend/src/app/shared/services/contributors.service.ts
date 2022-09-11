/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';

export class ContributorDto {
    public readonly _links: ResourceLinks;

    public readonly canRevoke: boolean;
    public readonly canUpdate: boolean;

    public get token() {
        return `subject:${this.contributorId}`;
    }

    constructor(links: ResourceLinks,
        public readonly contributorId: string,
        public readonly contributorName: string,
        public readonly contributorEmail: string,
        public readonly role: string,
    ) {
        this._links = links;

        this.canRevoke = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export type ContributorsDto = Versioned<ContributorsPayload>;

export type ContributorsPayload = Readonly<{
    // The list of contributors.
    items: ReadonlyArray<ContributorDto>;

    // The number of allowed contributors.
    maxContributors: number;

    // True, if the user has been invited.
    isInvited?: boolean;

    // True, if the user has permission to create a contributor.
    canCreate?: boolean;
}>;

export type AssignContributorDto = Readonly<{
    // The user ID.
    contributorId: string;

    // The role for the contributor.
    role: string;

    // True, if the user should be invited.
    invite?: boolean;
}>;

@Injectable()
export class ContributorsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
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
            pretifyError('i18n:contributors.addFailed'));
    }

    public deleteContributor(appName: string, resource: Resource, version: Version): Observable<ContributorsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseContributors(body);
            }),
            pretifyError('i18n:contributors.deleteFailed'));
    }
}

function parseContributors(response: { items: any[]; maxContributors: number } & Resource): ContributorsPayload {
    const { items: list, maxContributors, _meta, _links } = response;
    const items = list.map(parseContributor);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, maxContributors, canCreate, isInvited: _meta?.['isInvited'] === '1' };
}

function parseContributor(response: any) {
    return new ContributorDto(response._links,
        response.contributorId,
        response.contributorName,
        response.contributorEmail,
        response.role);
}