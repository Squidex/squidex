/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig } from 'framework';

export interface UISettingsDto {
    regexSuggestions: UIRegexSuggestionDto[];
}

export interface UIRegexSuggestionDto {
    name: string; pattern: string;
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
                    return Observable.of({ regexSuggestions: [] });
                })
                .do(settings => {
                    this.settings = settings;
                });
        }
    }
}