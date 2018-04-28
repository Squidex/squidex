/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import '@app/framework/angular/http/http-extensions';

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
            return Observable.of(this.settings);
        } else {
            const url = this.apiUrl.buildUrl(`api/ui/settings`);

            return this.http.get<UISettingsDto>(url)
                .catch(error => {
                    return Observable.of({ regexSuggestions: [], mapType: 'OSM', mapKey: '' });
                })
                .do(settings => {
                    this.settings = settings;
                });
        }
    }
}