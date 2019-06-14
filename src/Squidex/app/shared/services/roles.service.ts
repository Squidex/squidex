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

export type RolesDto = Versioned<RoleDto[]>;

export class RoleDto extends Model<RoleDto> {
    constructor(
        public readonly name: string,
        public readonly numClients: number,
        public readonly numContributors: number,
        public readonly permissions: string[]
    ) {
        super();
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
                    const items: any[] = body.roles;

                    const roles = items.map(item =>
                        new RoleDto(
                            item.name,
                            item.numClients,
                            item.numContributors,
                            item.permissions));

                    return roles;
                }),
                pretifyError('Failed to load roles. Please reload.'));
    }

    public postRole(appName: string, dto: CreateRoleDto, version: Version): Observable<Versioned<RoleDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
                mapVersioned(() => {
                    const role = new RoleDto(dto.name, 0, 0, []);

                    return role;
                }),
                tap(() => {
                    this.analytics.trackEvent('Role', 'Created', appName);
                }),
                pretifyError('Failed to add role. Please reload.'));
    }

    public putRole(appName: string, name: string, dto: UpdateRoleDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/${name}`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
                tap(() => {
                    this.analytics.trackEvent('Role', 'Updated', appName);
                }),
                pretifyError('Failed to revoke role. Please reload.'));
    }

    public deleteRole(appName: string, name: string, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles/${name}`);

        return HTTP.deleteVersioned(this.http, url, version).pipe(
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