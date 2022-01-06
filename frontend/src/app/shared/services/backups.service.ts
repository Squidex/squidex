/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AnalyticsService, ApiUrlConfig, DateTime, hasAnyLink, pretifyError, Resource, ResourceLinks, ResultSet, Types } from '@app/framework';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';

export class BackupsDto extends ResultSet<BackupDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }
}

export class BackupDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDownload: boolean;

    public readonly downloadUrl: string;

    public get isFailed() {
        return this.status === 'Failed';
    }

    constructor(
        links: ResourceLinks,
        public readonly id: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly handledEvents: number,
        public readonly handledAssets: number,
        public readonly status: 'Started' | 'Failed' | 'Success' | 'Completed' | 'Pending',
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDownload = !!stopped && !this.isFailed;

        this.downloadUrl = links['download'].href;
    }
}

export class RestoreDto {
    constructor(
        public readonly url: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly status: string,
        public readonly log: ReadonlyArray<string>,
    ) {
    }
}

export type StartRestoreDto =
    Readonly<{ url: string; newAppName?: string }>;

@Injectable()
export class BackupsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService,
    ) {
    }

    public getBackups(appName: string): Observable<BackupsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.get<{ items: any[]; _links: {} } & Resource>(url).pipe(
            map(({ items, _links }) => {
                const backups = items.map(parseBackup);

                return new BackupsDto(backups.length, backups, _links);
            }),
            pretifyError('i18n:backups.loadFailed'));
    }

    public getRestore(): Observable<RestoreDto | null> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.get(url).pipe(
            map(body => {
                const restore = parseRestore(body);

                return restore;
            }),
            catchError(error => {
                if (Types.is(error, HttpErrorResponse) && error.status === 404) {
                    return of(null);
                } else {
                    return throwError(() => error);
                }
            }),
            pretifyError('i18n:backups.loadFailed'));
    }

    public postBackup(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.post(url, {}).pipe(
            tap(() => {
                this.analytics.trackEvent('Backup', 'Started', appName);
            }),
            pretifyError('i18n:backups.startFailed'));
    }

    public postRestore(dto: StartRestoreDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.post(url, dto).pipe(
            tap(() => {
                this.analytics.trackEvent('Restore', 'Started');
            }),
            pretifyError('i18n:backups.restoreFailed'));
    }

    public deleteBackup(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            tap(() => {
                this.analytics.trackEvent('Backup', 'Deleted', appName);
            }),
            pretifyError('i18n:backups.deleteFailed'));
    }
}

function parseRestore(response: any) {
    return new RestoreDto(
        response.url,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.status,
        response.log);
}

function parseBackup(response: any) {
    return new BackupDto(response._links,
        response.id,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.handledEvents,
        response.handledAssets,
        response.status);
}
