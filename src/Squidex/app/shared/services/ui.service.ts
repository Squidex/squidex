/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import { ApiUrlConfig } from '@app/framework';

export interface UISettingsDto {
    mapType: string;
    mapKey: string;
}

@Injectable()
export class UIService {
    private settings: UISettingsDto;

    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getSettings(): Observable<UISettingsDto> {
        if (this.settings) {
            return of(this.settings);
        } else {
            const url = this.apiUrl.buildUrl(`api/ui/settings`);

            return this.http.get<UISettingsDto>(url).pipe(
                catchError(error => {
                    return of({ regexSuggestions: [], mapType: 'OSM', mapKey: '' });
                }),
                tap(settings => {
                    this.settings = settings;
                }));
        }
    }
}