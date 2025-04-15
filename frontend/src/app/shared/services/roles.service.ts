/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { AddRoleDto, IAddRoleDto, IUpdateRoleDto, RolesDto, UpdateRoleDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class RolesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getRoles(appName: string): Observable<Versioned<RolesDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return RolesDto.fromJSON(body);
            }),
            pretifyError('i18n:roles.loadFailed'));
    }

    public postRole(appName: string, dto: IAddRoleDto, version: VersionOrTag): Observable<Versioned<RolesDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.postVersioned(this.http, url, new AddRoleDto(dto).toJSON(), version).pipe(
            mapVersioned(({ body }) => {
                return RolesDto.fromJSON(body);
            }),
            pretifyError('i18n:roles.addFailed'));
    }

    public putRole(appName: string, resource: Resource, dto: IUpdateRoleDto, version: VersionOrTag): Observable<Versioned<RolesDto>> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateRoleDto(dto).toJSON()).pipe(
            mapVersioned(({ body }) => {
                return RolesDto.fromJSON(body);
            }),
            pretifyError('i18n:roles.updateFailed'));
    }

    public deleteRole(appName: string, resource: Resource, version: VersionOrTag): Observable<Versioned<RolesDto>> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return RolesDto.fromJSON(body);
            }),
            pretifyError('i18n:roles.revokeFailed'));
    }

    public getPermissions(appName: string): Observable<ReadonlyArray<string>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/permissions`);

        return this.http.get<string[]>(url).pipe(
            pretifyError('i18n:roles.loadPermissionsFailed'));
    }
}