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
    DateTime
} from 'framework';

export class BackupDto {
    constructor(
        public readonly id: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly handledEvents: number,
        public readonly handledAssets: number,
        public readonly isFailed: boolean
    ) {
    }
}

@Injectable()
export class BackupsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getBackups(appName: string): Observable<BackupDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.get(url)
                .map(response => {
                    const items: any[] = <any>response;

                    return items.map(item => {
                        return new BackupDto(
                            item.id,
                            DateTime.parseISO_UTC(item.started),
                            item.stopped ? DateTime.parseISO_UTC(item.stopped) : null,
                            item.handledEvents,
                            item.handledAssets,
                            item.isFailed);
                    });
                })
                .pretifyError('Failed to load backups.');
    }

    public postBackup(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.post(url, {})
                .do(() => {
                    this.analytics.trackEvent('Backup', 'Started', appName);
                })
                .pretifyError('Failed to start backup.');
    }

    public deleteBackup(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups/${id}`);

        return this.http.delete(url)
                .do(() => {
                    this.analytics.trackEvent('Backup', 'Deleted', appName);
                })
                .pretifyError('Failed to delete backup.');
    }
}