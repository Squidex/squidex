/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ApiUrlConfig } from '@app/framework';

export interface UISettingsDto {
    readonly canCreateApps: boolean;
}

@Injectable()
export class UIService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getCommonSettings(): Observable<UISettingsDto> {
        const url = this.apiUrl.buildUrl(`api/ui/settings`);

        return this.http.get<UISettingsDto>(url).pipe(
            catchError(() => {
                return of({ mapType: 'OSM', mapKey: '', canCreateApps: true });
            }));
    }

    public getSettings(appName: string): Observable<object> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings`);

        return this.http.get<object>(url).pipe(
            catchError(() => {
                return of({});
            }));
    }

    public putSetting(appName: string, key: string, value: any): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/${key}`);

        return this.http.put(url, { value });
    }

    public deleteSetting(appName: string, key: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/${key}`);

        return this.http.delete(url);
    }
}