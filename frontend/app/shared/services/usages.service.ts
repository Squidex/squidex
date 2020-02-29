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

export class CallsUsageDto {
    constructor(
        public readonly allowedCalls: number,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly averageElapsedMs: number,
        public readonly details: { [category: string]: ReadonlyArray<CallsUsagePerDateDto> }
    ) {
    }
}

export class CallsUsagePerDateDto {
    constructor(
        public readonly date: DateTime,
        public readonly totalBytes: number,
        public readonly totalCalls: number,
        public readonly averageElapsedMs: number
    ) {
    }
}

export class StorageUsagePerDateDto {
    constructor(
        public readonly date: DateTime,
        public readonly totalCount: number,
        public readonly totalSize: number
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

    public getCallsUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<CallsUsageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/${fromDate.toUTCStringFormat('YYYY-MM-DD')}/${toDate.toUTCStringFormat('YYYY-MM-DD')}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const details: { [category: string]: CallsUsagePerDateDto[] } = {};

                for (let category of Object.keys(body.details)) {
                    details[category] = body.details[category].map((item: any) =>
                        new CallsUsagePerDateDto(
                            DateTime.parseISO_UTC(item.date),
                            item.totalBytes,
                            item.totalCalls,
                            item.averageElapsedMs));
                }

                const usages =
                    new CallsUsageDto(
                        body.allowedCalls,
                        body.totalBytes,
                        body.totalCalls,
                        body.averageElapsedMs,
                        details);

                return usages;
            }),
            pretifyError('Failed to load calls usage. Please reload.'));
    }

    public getStorageUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<ReadonlyArray<StorageUsagePerDateDto>> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/${fromDate.toUTCStringFormat('YYYY-MM-DD')}/${toDate.toUTCStringFormat('YYYY-MM-DD')}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const usages = body.map(item =>
                    new StorageUsagePerDateDto(
                        DateTime.parseISO_UTC(item.date),
                        item.totalCount,
                        item.totalSize));

                return usages;
            }),
            pretifyError('Failed to load storage usage. Please reload.'));
    }
}