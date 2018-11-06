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

export class AppRolesDto extends Model {
    constructor(
        public readonly roles: AppRoleDto[],
        public readonly version: Version
    ) {
        super();
    }
}

export class AppRoleDto extends Model {
    constructor(
        public readonly name: string,
        public readonly numClients: number,
        public readonly numContributors: number,
        public readonly permissions: string[]
    ) {
        super();
    }

    public with(value: Partial<AppRoleDto>): AppRoleDto {
        return this.clone(value);
    }
}

export class CreateAppRoleDto {
    constructor(
        public readonly name: string
    ) {
    }
}

export class UpdateAppRoleDto {
    constructor(
        public readonly permissions: string[]
    ) {
    }
}

@Injectable()
export class AppRolesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getRoles(appName: string): Observable<AppRolesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.getVersioned<any>(this.http, url).pipe(
                map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.roles;

                    const roles = items.map(item => {
                        return new AppRoleDto(
                            item.name,
                            item.numClients,
                            item.numContributors,
                            item.permissions);
                    });

                    return new AppRolesDto(roles, response.version);
                }),
                pretifyError('Failed to load roles. Please reload.'));
    }

    public postRole(appName: string, dto: CreateAppRoleDto, version: Version): Observable<Versioned<AppRoleDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/roles`);

        return HTTP.postVersioned<any>(this.http, url, dto, version).pipe(
                map(response => {
                    const role = new AppRoleDto(dto.name, 0, 0, []);

                    return new Versioned(response.version, role);
                }),
                tap(() => {
                    this.analytics.trackEvent('Role', 'Created', appName);
                }),
                pretifyError('Failed to add role. Please reload.'));
    }

    public putRole(appName: string, name: string, dto: UpdateAppRoleDto, version: Version): Observable<Versioned<any>> {
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