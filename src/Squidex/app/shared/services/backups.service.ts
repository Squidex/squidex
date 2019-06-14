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
    Resource,
    ResourceLinks,
    ResultSet,
    Types,
    withLinks
} from '@app/framework';

export class BackupsDto extends ResultSet<BackupDto> {
    public readonly _links: ResourceLinks = {};
}

export class BackupDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly id: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly handledEvents: number,
        public readonly handledAssets: number,
        public readonly status: string
    ) {
    }
}

export class RestoreDto extends Model<BackupDto> {
    constructor(
        public readonly url: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly status: string,
        public readonly log: string[]
    ) {
        super();
    }
}

export interface StartRestoreDto {
    readonly url: string;
    readonly newAppName?: string;
}

@Injectable()
export class BackupsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getBackups(appName: string): Observable<BackupsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.get<{ items: any[] } & Resource>(url).pipe(
                map(body => {
                    const backups = body.items.map(item => parseBackup(item));

                    return withLinks(new BackupsDto(backups.length, backups), body);
                }),
                pretifyError('Failed to load backups.'));
    }

    public getRestore(): Observable<RestoreDto | null> {
        const url = this.apiUrl.buildUrl(`api/apps/restore`);

        return this.http.get(url).pipe(
                map(body => {
                    const restore = parseRestore(body);

                    return restore;
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

    public deleteBackup(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
                tap(() => {
                    this.analytics.trackEvent('Backup', 'Deleted', appName);
                }),
                pretifyError('Failed to delete backup.'));
    }
}

function parseRestore(response: any) {
    return new RestoreDto(
        response.url,
        DateTime.parseISO_UTC(response.started),
        response.stopped ? DateTime.parseISO_UTC(response.stopped) : null,
        response.status,
        response.log);
}

function parseBackup(response: any) {
    return withLinks(
        new BackupDto(
            response.id,
            DateTime.parseISO_UTC(response.started),
            response.stopped ? DateTime.parseISO_UTC(response.stopped) : null,
            response.handledEvents,
            response.handledAssets,
            response.status),
        response);
}