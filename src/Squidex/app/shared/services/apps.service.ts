/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP
} from 'framework';

export class AppDto {
    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly permission: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly planName: string,
        public readonly planUpgrade: string
    ) {
    }
}

export class CreateAppDto {
    constructor(
        public readonly name: string,
        public readonly template?: string
    ) {
    }
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

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body;

                    return items.map(item => {
                        return new AppDto(
                            item.id,
                            item.name,
                            item.permission,
                            DateTime.parseISO(item.created),
                            DateTime.parseISO(item.lastModified),
                            item.planName,
                            item.planUpgrade);
                    });
                })
                .pretifyError('Failed to load apps. Please reload.');
    }

    public postApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return HTTP.postVersioned<any>(this.http, url, dto)
                .map(response => {
                    const body = response.payload.body;

                    now = now || DateTime.now();

                    return new AppDto(body.id, dto.name, body.permission, now, now, body.planName, body.planUpgrade);
                })
                .do(() => {
                    this.analytics.trackEvent('App', 'Created', dto.name);
                })
                .pretifyError('Failed to create app. Please reload.');
    }

    public deleteApp(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}`);

        return this.http.delete(url)
                .do(() => {
                    this.analytics.trackEvent('App', 'Archived', appName);
                })
                .pretifyError('Failed to archive app. Please reload.');
    }
}