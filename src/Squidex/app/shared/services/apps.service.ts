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
    pretifyError,
    Resource,
    ResourceLinks,
    withLinks
} from '@app/framework';

export class AppDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly id: string,
        public readonly name: string,
        public readonly permissions: string[],
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly canAccessApi: boolean,
        public readonly canAccessContent: boolean,
        public readonly planName?: string,
        public readonly planUpgrade?: string
    ) {
    }
}

export interface CreateAppDto {
    readonly name: string;
    readonly template?: string;
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
                    const apps = body.map(item => parseApp(item));

                    return apps;
                }),
                pretifyError('Failed to load apps. Please reload.'));
    }

    public postApp(dto: CreateAppDto): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return this.http.post<any>(url, dto).pipe(
                map(body => {
                    return parseApp(body);
                }),
                tap(() => {
                    this.analytics.trackEvent('App', 'Created', dto.name);
                }),
                pretifyError('Failed to create app. Please reload.'));
    }

    public deleteApp(resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
                tap(() => {
                    this.analytics.trackEvent('App', 'Archived');
                }),
                pretifyError('Failed to archive app. Please reload.'));
    }
}

function parseApp(response: any) {
    return withLinks(
        new AppDto(
            response.id,
            response.name,
            response.permissions,
            DateTime.parseISO(response.created),
            DateTime.parseISO(response.lastModified),
            response.canAccessApi,
            response.canAccessContent,
            response.planName,
            response.planUpgrade),
        response);
}
