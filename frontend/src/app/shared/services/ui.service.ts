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

export type UISettingsDto = Readonly<{
    // True, if the user has the permissions to create a new app.
    canCreateApps?: boolean;
}>;

@Injectable()
export class UIService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getCommonSettings(): Observable<UISettingsDto> {
        const url = this.apiUrl.buildUrl('api/ui/settings');

        return this.http.get<UISettingsDto>(url).pipe(
            catchError(() => {
                return of({ mapType: 'OSM', mapKey: '', canCreateApps: true });
            }));
    }

    public getSharedSettings(appName: string): Observable<{}> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings`);

        return this.http.get<{}>(url).pipe(
            catchError(() => {
                return of({});
            }));
    }

    public getUserSettings(appName: string): Observable<{}> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/me`);

        return this.http.get<{}>(url).pipe(
            catchError(() => {
                return of({});
            }));
    }

    public putSharedSetting(appName: string, key: string, value: any): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/${key}`);

        return this.http.put(url, { value });
    }

    public putUserSetting(appName: string, key: string, value: any): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/me/${key}`);

        return this.http.put(url, { value });
    }

    public deleteSharedSetting(appName: string, key: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/${key}`);

        return this.http.delete(url);
    }

    public deleteUserSetting(appName: string, key: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ui/settings/me/${key}`);

        return this.http.delete(url);
    }
}
