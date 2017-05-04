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

export class UsageDto {
    constructor(
        public readonly date: DateTime,
        public readonly count: number,
        public readonly averageMs: number
    ) {
    }
}

export class MonthlyCallsDto {
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

    public getMonthlyCalls(app: string): Observable<MonthlyCallsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/monthly`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    return new MonthlyCallsDto(response.count);
                })
                .catchError('Failed to load monthly calls. Please reload.');
    }

    public getUsages(app: string, fromDate: DateTime, toDate: DateTime): Observable<UsageDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${app}/usages/${fromDate.toStringFormat('YYYY-MM-DD')}/${toDate.toStringFormat('YYYY-MM-DD')}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new UsageDto(
                            DateTime.parseISO_UTC(item.date),
                            item.count,
                            item.averageMs);
                    });
                })
                .catchError('Failed to load usage. Please reload.');
    }
}