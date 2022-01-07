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

export class RoleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    constructor(
        links: ResourceLinks,
        public readonly name: string,
        public readonly numClients: number,
        public readonly numContributors: number,
        public readonly permissions: ReadonlyArray<string>,
        public readonly properties: {},
        public readonly isDefaultRole: boolean,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

type Permissions = readonly string[];

export type RolesDto =
    Versioned<RolesPayload>;

export type RolesPayload =
    Readonly<{ items: ReadonlyArray<RoleDto>; canCreate: boolean } & Resource>;

export type CreateRoleDto =
    Readonly<{ name: string }>;

export type UpdateRoleDto =
    Readonly<{ permissions: Permissions; properties: {} }>;

@Injectable()
export class RolesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getRoles(appName: string): Observable<RolesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
            }),
            pretifyError('i18n:roles.loadFailed'));
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
            pretifyError('i18n:roles.addFailed'));
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
            pretifyError('i18n:roles.updateFailed'));
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
            pretifyError('i18n:roles.revokeFailed'));
    }

    public getPermissions(appName: string): Observable<ReadonlyArray<string>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/permissions`);

        return this.http.get<string[]>(url).pipe(
            pretifyError('i18n:roles.loadPermissionsFailed'));
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
            item.properties,
            item.isDefaultRole));

    const _links = response._links;

    return { items, _links, canCreate: hasAnyLink(_links, 'create') };
}
