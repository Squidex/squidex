/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

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
        public readonly lastModified: DateTime
    ) {
    }
}

export class CreateAppDto {
    constructor(
        public readonly name: string
    ) {
    }
}

@Injectable()
export class AppsService {
    private apps$: ReplaySubject<AppDto[]> | null = null;

    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getApps(): Observable<AppDto[]> {
        if (this.apps$ === null) {
            const url = this.apiUrl.buildUrl('/api/apps');

            const loadedApps =
                HTTP.getVersioned<any>(this.http, url)
                    .map(response => {
                        const body = response.payload.body;

                        const items: any[] = body;

                        return items.map(item => {
                            return new AppDto(
                                item.id,
                                item.name,
                                item.permission,
                                DateTime.parseISO(item.created),
                                DateTime.parseISO(item.lastModified));
                        });
                    })
                    .pretifyError('Failed to load apps. Please reload.');

            this.apps$ = new ReplaySubject<AppDto[]>(1);

            loadedApps
                .subscribe(apps => {
                    this.apps$.next(apps);
                }, error => {
                    this.apps$.error(loadedApps);
                });
        }

        return this.apps$;
    }

    public postApp(dto: CreateAppDto, now?: DateTime): Observable<AppDto> {
        const url = this.apiUrl.buildUrl('api/apps');

        return HTTP.postVersioned<any>(this.http, url, dto)
                .map(response => {
                    const body = response.payload.body;

                    now = now || DateTime.now();

                    return new AppDto(body.id, dto.name, 'Owner', now, now);
                })
                .do(app => {
                    this.analytics.trackEvent('App', 'Created', dto.name);

                    if (this.apps$) {
                        this.apps$.first().subscribe(apps => {
                            this.apps$.next(apps.concat([app]));
                        });
                    }
                })
                .pretifyError('Failed to create app. Please reload.');
    }
}