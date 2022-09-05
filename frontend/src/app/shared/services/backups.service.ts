/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiUrlConfig, DateTime, hasAnyLink, pretifyError, Resource, ResourceLinks, Types } from '@app/framework';

export class BackupDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDownload: boolean;

    public readonly downloadUrl: string;

    public get isFailed() {
        return this.status === 'Failed';
    }

    constructor(links: ResourceLinks,
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

export type BackupsDto = Readonly<{
    // The list of backups.
    items: ReadonlyArray<BackupDto>;

    // True, if the user has permissions to create a backup.
    canCreate?: boolean;
}>;

export type StartRestoreDto = Readonly<{
    // The url of the backup file.
    url: string;

    // The optional app name tro use.
    newAppName?: string;
}>;

@Injectable()
export class BackupsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getBackups(appName: string): Observable<BackupsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.get<{ items: any[]; _links: {} } & Resource>(url).pipe(
            map(body => {
                return parseBackups(body);
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
            pretifyError('i18n:backups.startFailed'));
    }

    public postRestore(dto: StartRestoreDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.post(url, dto).pipe(
            pretifyError('i18n:backups.restoreFailed'));
    }

    public deleteBackup(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:backups.deleteFailed'));
    }
}

function parseBackups(response: { items: any[] } & Resource): BackupsDto {
    const { items: list, _links } = response;
    const items = list.map(parseBackup);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, canCreate };
}

function parseRestore(response: any) {
    return new RestoreDto(
        response.url,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.status,
        response.log);
}

function parseBackup(response: any & Resource) {
    return new BackupDto(response._links,
        response.id,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.handledEvents,
        response.handledAssets,
        response.status);
}
