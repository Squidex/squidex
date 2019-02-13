/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    Model,
    pretifyError,
    Types
} from '@app/framework';

export class BackupDto extends Model {
    constructor(
        public readonly id: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly handledEvents: number,
        public readonly handledAssets: number,
        public readonly status: string
    ) {
        super();
    }
}

export class RestoreDto {
    constructor(
        public readonly url: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly status: string,
        public readonly log: string[]
    ) {
    }
}

export class StartRestoreDto {
    constructor(
        public readonly url: string,
        public readonly newAppName?: string
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

        return this.http.get<any[]>(url).pipe(
                map(response => {
                    return response.map(item => {
                        return new BackupDto(
                            item.id,
                            DateTime.parseISO_UTC(item.started),
                            item.stopped ? DateTime.parseISO_UTC(item.stopped) : null,
                            item.handledEvents,
                            item.handledAssets,
                            item.status);
                    });
                }),
                pretifyError('Failed to load backups.'));
    }

    public getRestore(): Observable<RestoreDto | null> {
        const url = this.apiUrl.buildUrl(`api/apps/restore`);

        return this.http.get<any>(url).pipe(
                map(response => {
                    return new RestoreDto(
                        response.url,
                        DateTime.parseISO_UTC(response.started),
                        response.stopped ? DateTime.parseISO_UTC(response.stopped) : null,
                        response.status,
                        response.log);
                }),
                catchError(error => {
                    if (Types.is(error, HttpErrorResponse) && error.status === 404) {
                        return of(null);
                    } else {
                        return throwError(error);
                    }
                }),
                pretifyError('Failed to load backups.'));
    }

    public postBackup(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.post(url, {}).pipe(
                tap(() => {
                    this.analytics.trackEvent('Backup', 'Started', appName);
                }),
                pretifyError('Failed to start backup.'));
    }

    public postRestore(dto: StartRestoreDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/restore`);

        return this.http.post(url, dto).pipe(
                tap(() => {
                    this.analytics.trackEvent('Restore', 'Started');
                }),
                pretifyError('Failed to start restore.'));
    }

    public deleteBackup(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups/${id}`);

        return this.http.delete(url).pipe(
                tap(() => {
                    this.analytics.trackEvent('Backup', 'Deleted', appName);
                }),
                pretifyError('Failed to delete backup.'));
    }
}