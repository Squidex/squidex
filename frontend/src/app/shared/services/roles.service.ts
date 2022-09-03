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

export class RoleDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
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

export type RolesDto = Versioned<RolesPayload>;

export type RolesPayload = Readonly<{
    // The list of roles.
    items: ReadonlyArray<RoleDto>;

    // True, if the user has permissions to create a new role.
    canCreate?: boolean;
}>;

export type CreateRoleDto = Readonly<{
    // The name of the role, cannot be changed later.
    name: string;
}>;

export type UpdateRoleDto = Readonly<{
    // The permissions in dot notation.
    permissions: ReadonlyArray<string>;

    // The UI properties.
    properties: {};
}>;

@Injectable()
export class RolesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
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
            pretifyError('i18n:roles.addFailed'));
    }

    public putRole(appName: string, resource: Resource, dto: UpdateRoleDto, version: Version): Observable<RolesDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseRoles(body);
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
            pretifyError('i18n:roles.revokeFailed'));
    }

    public getPermissions(appName: string): Observable<ReadonlyArray<string>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/permissions`);

        return this.http.get<string[]>(url).pipe(
            pretifyError('i18n:roles.loadPermissionsFailed'));
    }
}

function parseRoles(response: { items: any } & Resource): RolesPayload {
    const { items: list, _links } = response;
    const items = list.map(parseRole);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
}

function parseRole(response: any) {
    return new RoleDto(response._links,
        response.name,
        response.numClients,
        response.numContributors,
        response.permissions,
        response.properties,
        response.isDefaultRole);
}