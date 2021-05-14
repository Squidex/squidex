/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, DateTime, pretifyError } from '@app/framework';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class CallsUsageDto {
    constructor(
        public readonly allowedBytes: number,
        public readonly allowedCalls: number,
        public readonly blockingCalls: number,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly monthBytes: number,
        public readonly monthCalls: number,
        public readonly averageElapsedMs: number,
        public readonly details: { [category: string]: ReadonlyArray<CallsUsagePerDateDto> },
    ) {
    }
}

export class CallsUsagePerDateDto {
    constructor(
        public readonly date: DateTime,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly averageElapsedMs: number,
    ) {
    }
}

export class StorageUsagePerDateDto {
    constructor(
        public readonly date: DateTime,
        public readonly totalCount: number,
        public readonly totalSize: number,
    ) {
    }
}

export class CurrentStorageDto {
    constructor(
        public readonly size: number,
        public readonly maxAllowed: number,
    ) {
    }
}

@Injectable()
export class UsagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getLog(app: string): Observable<string> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/log`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return body.downloadUrl;
            }),
            pretifyError('i18n:usages.loadMonthlyCallsFailed'));
    }

    public getTodayStorage(app: string): Observable<CurrentStorageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/today`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return new CurrentStorageDto(body.size, body.maxAllowed);
            }),
            pretifyError('i18n:usages.loadTodayStorageFailed'));
    }

    public getCallsUsages(app: string, fromDate: string, toDate: string): Observable<CallsUsageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/${fromDate}/${toDate}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const details: { [category: string]: CallsUsagePerDateDto[] } = {};

                for (const category of Object.keys(body.details)) {
                    details[category] = body.details[category].map((item: any) =>
                        new CallsUsagePerDateDto(
                            DateTime.parseISO(item.date),
                            item.totalBytes,
                            item.totalCalls,
                            item.averageElapsedMs));
                }

                const usages =
                    new CallsUsageDto(
                        body.allowedBytes,
                        body.allowedCalls,
                        body.blockingCalls,
                        body.totalBytes,
                        body.totalCalls,
                        body.monthBytes,
                        body.monthCalls,
                        body.averageElapsedMs,
                        details);

                return usages;
            }),
            pretifyError('i18n:usages.loadCallsFailed'));
    }

    public getStorageUsages(app: string, fromDate: string, toDate: string): Observable<ReadonlyArray<StorageUsagePerDateDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/${fromDate}/${toDate}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const usages = body.map(item =>
                    new StorageUsagePerDateDto(
                        DateTime.parseISO(item.date),
                        item.totalCount,
                        item.totalSize));

                return usages;
            }),
            pretifyError('i18n:usages.loadStorageFailed'));
    }
}
