/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, DateTime } from 'framework';
import { AuthService } from './auth.service';

export class CallsUsageDto {
    constructor(
        public readonly date: DateTime,
        public readonly count: number,
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
        public readonly size: number
    ) {
    }
}

export class CurrentCallsDto {
    constructor(
        public readonly count: number
    ) {
    }
}

@Injectable()
export class UsagesService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getMonthCalls(app: string): Observable<CurrentCallsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/month`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => new CurrentCallsDto(response.count))
                .catchError('Failed to load monthly api calls. Please reload.');
    }

    public getCallsUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<CallsUsageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/calls/${fromDate.toStringFormat('YYYY-MM-DD')}/${toDate.toStringFormat('YYYY-MM-DD')}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new CallsUsageDto(
                            DateTime.parseISO_UTC(item.date),
                            item.count,
                            item.averageMs);
                    });
                })
                .catchError('Failed to load calls usage. Please reload.');
    }

    public getTodayStorage(app: string): Observable<CurrentStorageDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/today`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => new CurrentStorageDto(response.size))
                .catchError('Failed to load todays storage size. Please reload.');
    }

    public getStorageUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<StorageUsageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/storage/${fromDate.toStringFormat('YYYY-MM-DD')}/${toDate.toStringFormat('YYYY-MM-DD')}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new StorageUsageDto(
                            DateTime.parseISO_UTC(item.date),
                            item.count,
                            item.size);
                    });
                })
                .catchError('Failed to load storage usage. Please reload.');
    }
}