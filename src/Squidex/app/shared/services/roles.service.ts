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

export type RolesDto = Versioned<RolesPayload>;
export type RolesPayload = {
    readonly items: RoleDto[];

    readonly canCreate: boolean;
} & Resource;

export class RoleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    constructor(
        links: ResourceLinks,
        public readonly name: string,
        public readonly numClients: number,
        public readonly numContributors: number,
        public readonly permissions: string[],
        public readonly isDefaultRole: boolean
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export interface CreateRoleDto {
    readonly name: string;
}

export interface UpdateRoleDto {
    readonly permissions: string[];
}

@Injectable()
export class RolesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getRoles(appName: string): Observable<RolesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
            }),
            pretifyError('Failed to load roles. Please reload.'));
    }

    public postRole(appName: string, dto: CreateRoleDto, version: Version): Observable<RolesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Role', 'Created', appName);
            }),
            pretifyError('Failed to add role. Please reload.'));
    }

    public putRole(appName: string, resource: Resource, dto: UpdateRoleDto, version: Version): Observable<RolesDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Role', 'Updated', appName);
            }),
            pretifyError('Failed to revoke role. Please reload.'));
    }

    public deleteRole(appName: string, resource: Resource, version: Version): Observable<RolesDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Role', 'Deleted', appName);
            }),
            pretifyError('Failed to revoke role. Please reload.'));
    }

    public getPermissions(appName: string): Observable<string[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/permissions`);

        return this.http.get<string[]>(url).pipe(
            pretifyError('Failed to load permissions. Please reload.'));
    }
}

export function parseRoles(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new RoleDto(item._links,
            item.name,
            item.numClients,
            item.numContributors,
            item.permissions,
            item.isDefaultRole));

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}