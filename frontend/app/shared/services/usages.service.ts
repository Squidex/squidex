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

import {
    ApiUrlConfig,
    DateTime,
    pretifyError
} from '@app/framework';

export class ApiUsagesDto {
    constructor(
        public readonly allowedCalls: number,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly averageMs: number,
        public readonly details: { [category: string]: ReadonlyArray<ApiUsageDto> }
    ) {
    }
}

export class ApiUsageDto {
    constructor(
        public readonly date: DateTime,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly averageMs: number
    ) {
    }
}

export class StorageUsageDto {
    constructor(
        public readonly date: DateTime,
        public readonly count: number,
        public readonly size: number
    ) {
    }
}

export class CurrentStorageDto {
    constructor(
        public readonly size: number,
        public readonly maxAllowed: number
    ) {
    }
}

@Injectable()
export class UsagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLog(app: string): Observable<string> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/log`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return body.downloadUrl;
            }),
            pretifyError('Failed to load monthly api calls. Please reload.'));
    }

    public getTodayStorage(app: string): Observable<CurrentStorageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/today`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return new CurrentStorageDto(body.size, body.maxAllowed);
            }),
            pretifyError('Failed to load todays storage size. Please reload.'));
    }

    public getCallsUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<ApiUsagesDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/${fromDate.toUTCStringFormat('YYYY-MM-DD')}/${toDate.toUTCStringFormat('YYYY-MM-DD')}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const details: { [category: string]: ApiUsageDto[] } = {};

                for (let category of Object.keys(body.details)) {
                    details[category] = body.details[category].map((item: any) =>
                        new ApiUsageDto(
                            DateTime.parseISO_UTC(item.date),
                            item.totalBytes,
                            item.totalCalls,
                            item.averageMs));
                }

                const usages =
                    new ApiUsagesDto(
                        body.allowedCalls,
                        body.totalBytes,
                        body.totalBytes,
                        body.averageMs,
                        details);

                return usages;
            }),
            pretifyError('Failed to load calls usage. Please reload.'));
    }

    public getStorageUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<ReadonlyArray<StorageUsageDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/${fromDate.toUTCStringFormat('YYYY-MM-DD')}/${toDate.toUTCStringFormat('YYYY-MM-DD')}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const usages = body.map(item =>
                    new StorageUsageDto(
                        DateTime.parseISO_UTC(item.date),
                        item.count,
                        item.size));

                return usages;
            }),
            pretifyError('Failed to load storage usage. Please reload.'));
    }
}