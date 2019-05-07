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
    DateTime,
    Model,
    Permission,
    pretifyError
} from '@app/framework';

export class AppDto extends Model<AppDto> {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly permissions: Permission[],
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly planName?: string,
        public readonly planUpgrade?: string
    ) {
        super();
    }
}

export interface CreateAppDto {
    readonly name: string;
    readonly template?: string;
}

export interface AppCreatedDto {
    readonly id: string;
    readonly permissions: string[];
    readonly planName?: string;
    readonly planUpgrade?: string;
}

@Injectable()
export class AppsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getApps(): Observable<AppDto[]> {
        const url = this.apiUrl.buildUrl('/api/apps');

        return this.http.get<any[]>(url).pipe(
                map(body => {
                    const apps = body.map(item => {
                        const permissions = (<string[]>item.permissions).map(x => new Permission(x));

                        return new AppDto(
                            item.id,
                            item.name,
                            permissions,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified),
                            item.planName,
                            item.planUpgrade);
                    });

                    return apps;
                }),
                pretifyError('Failed to load apps. Please reload.'));
    }

    public postApp(dto: CreateAppDto): Observable<AppCreatedDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return this.http.post<AppCreatedDto>(url, dto).pipe(
                tap(() => {
                    this.analytics.trackEvent('App', 'Created', dto.name);
                }),
                pretifyError('Failed to create app. Please reload.'));
    }

    public deleteApp(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}`);

        return this.http.delete(url).pipe(
                tap(() => {
                    this.analytics.trackEvent('App', 'Archived', appName);
                }),
                pretifyError('Failed to archive app. Please reload.'));
    }
}