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

export class JobDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canDownload: boolean;

    public readonly downloadUrl?: string;

    public get isFailed() {
        return this.status === 'Failed';
    }

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly started: DateTime,
        public readonly stopped: DateTime | null,
        public readonly taskName: string,
        public readonly taskArguments: Record<string, string>,
        public readonly description: string,
        public readonly log: ReadonlyArray<JobLogMessageDto>,
        public readonly status: 'Started' | 'Failed' | 'Success' | 'Completed' | 'Pending',
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canDownload = hasAnyLink(links, 'download');

        this.downloadUrl = links['download']?.href;
    }
}

export class JobLogMessageDto {
    constructor(
        public readonly timestamp: DateTime,
        public readonly message: string,
    ) {
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

export type JobsDto = Readonly<{
    // The list of jobs.
    items: ReadonlyArray<JobDto>;

    // True, if the user has permissions to create a backup.
    canCreateBackup?: boolean;
}>;

export type StartRestoreDto = Readonly<{
    // The url of the backup file.
    url: string;

    // The optional app name tro use.
    newAppName?: string;
}>;

@Injectable({
    providedIn: 'root',
})
export class JobsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getJobs(appName: string): Observable<JobsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/jobs`);

        return this.http.get<{ items: any[]; _links: {} } & Resource>(url).pipe(
            map(body => {
                return parseJobs(body);
            }),
            pretifyError('i18n:jobs.loadFailed'));
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
            pretifyError('i18n:jobs.loadFailed'));
    }

    public postBackup(appName: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/backups`);

        return this.http.post(url, {}).pipe(
            pretifyError('i18n:jobs.backupFailed'));
    }

    public postRestore(dto: StartRestoreDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.post(url, dto).pipe(
            pretifyError('i18n:jobs.restoreFailed'));
    }

    public deleteJob(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:jobs.deleteFailed'));
    }
}

function parseJobs(response: { items: any[] } & Resource): JobsDto {
    const { items: list, _links } = response;
    const items = list.map(parseJob);

    const canCreateBackup = hasAnyLink(_links, 'create/backups');

    return { items, canCreateBackup };
}

function parseRestore(response: any) {
    return new RestoreDto(
        response.url,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.status,
        response.log);
}

function parseJob(response: any & Resource) {
    const log: any[] = response.log;

    return new JobDto(response._links,
        response.id,
        DateTime.parseISO(response.started),
        response.stopped ? DateTime.parseISO(response.stopped) : null,
        response.taskName,
        response.taskArguments,
        response.description,
        log.map(x => new JobLogMessageDto(DateTime.parseISO(x.timestamp), x.message)),
        response.status);
}
