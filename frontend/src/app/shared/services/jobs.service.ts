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
import { ApiUrlConfig, pretifyError, Resource, Types } from '@app/framework';
import { IRestoreRequestDto, JobsDto, RestoreJobDto, RestoreRequestDto } from './../model';


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
                return JobsDto.fromJSON(body);
            }),
            pretifyError('i18n:jobs.loadFailed'));
    }

    public getRestore(): Observable<RestoreJobDto | null> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.get(url).pipe(
            map(body => {
                return RestoreJobDto.fromJSON(body);
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

    public postRestore(dto: IRestoreRequestDto): Observable<any> {
        const url = this.apiUrl.buildUrl('api/apps/restore');

        return this.http.post(url, new RestoreRequestDto(dto).toJSON()).pipe(
            pretifyError('i18n:jobs.restoreFailed'));
    }

    public deleteJob(appName: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:jobs.deleteFailed'));
    }
}