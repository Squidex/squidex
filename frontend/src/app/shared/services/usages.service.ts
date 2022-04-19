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
import { ApiUrlConfig, DateTime, pretifyError } from '@app/framework';

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
                return parseCurrentStorage(body);
            }),
            pretifyError('i18n:usages.loadTodayStorageFailed'));
    }

    public getCallsUsages(app: string, fromDate: string, toDate: string): Observable<CallsUsageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/${fromDate}/${toDate}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseCallsUsage(body);
            }),
            pretifyError('i18n:usages.loadCallsFailed'));
    }

    public getStorageUsages(app: string, fromDate: string, toDate: string): Observable<ReadonlyArray<StorageUsagePerDateDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/${fromDate}/${toDate}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return parseStorageUser(body);
            }),
            pretifyError('i18n:usages.loadStorageFailed'));
    }
}

function parseCurrentStorage(response: any): CurrentStorageDto {
    return new CurrentStorageDto(response.size, response.maxAllowed);
}

function parseCallsUsage(response: any) {
    const details: { [category: string]: CallsUsagePerDateDto[] } = {};

    for (const [category, value] of Object.entries(response.details)) {
        details[category] = (value as any).map((item: any) => new CallsUsagePerDateDto(
            DateTime.parseISO(item.date),
            item.totalBytes,
            item.totalCalls,
            item.averageElapsedMs));
    }

    const usages = new CallsUsageDto(
        response.allowedBytes,
        response.allowedCalls,
        response.blockingCalls,
        response.totalBytes,
        response.totalCalls,
        response.monthBytes,
        response.monthCalls,
        response.averageElapsedMs,
        details);

    return usages;
}

function parseStorageUser(response: any[]) {
    const usages = response.map(item => new StorageUsagePerDateDto(
        DateTime.parseISO(item.date),
        item.totalCount,
        item.totalSize));

    return usages;
}
