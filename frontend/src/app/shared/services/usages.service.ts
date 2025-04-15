/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, pretifyError } from '@app/framework';
import { CallsUsageDto, CurrentStorageDto, StorageUsagePerDateDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class UsagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getLog(appName: string): Observable<string> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/usages/log`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return body.downloadUrl;
            }),
            pretifyError('i18n:usages.loadMonthlyCallsFailed'));
    }

    public getTodayStorage(appName: string): Observable<CurrentStorageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/usages/storage/today`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return CurrentStorageDto.fromJSON(body);
            }),
            pretifyError('i18n:usages.loadTodayStorageFailed'));
    }

    public getTodayStorageForTeam(teamId: string): Observable<CurrentStorageDto> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/usages/storage/today`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return CurrentStorageDto.fromJSON(body);
            }),
            pretifyError('i18n:usages.loadTodayStorageFailed'));
    }

    public getCallsUsages(appName: string, fromDate: string, toDate: string): Observable<CallsUsageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/usages/calls/${fromDate}/${toDate}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return CallsUsageDto.fromJSON(body);
            }),
            pretifyError('i18n:usages.loadCallsFailed'));
    }

    public getCallsUsagesForTeam(teamId: string, fromDate: string, toDate: string): Observable<CallsUsageDto> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/usages/calls/${fromDate}/${toDate}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return CallsUsageDto.fromJSON(body);
            }),
            pretifyError('i18n:usages.loadCallsFailed'));
    }

    public getStorageUsages(appName: string, fromDate: string, toDate: string): Observable<ReadonlyArray<StorageUsagePerDateDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/usages/storage/${fromDate}/${toDate}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(StorageUsagePerDateDto.fromJSON);
            }),
            pretifyError('i18n:usages.loadStorageFailed'));
    }

    public getStorageUsagesForTeam(teamId: string, fromDate: string, toDate: string): Observable<ReadonlyArray<StorageUsagePerDateDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/usages/storage/${fromDate}/${toDate}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(StorageUsagePerDateDto.fromJSON);
            }),
            pretifyError('i18n:usages.loadStorageFailed'));
    }
}