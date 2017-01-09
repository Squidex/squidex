/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig } from 'framework';

import { AuthService } from './auth.service';

export class LanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string
    ) {
    }
}

@Injectable()
export class LanguageService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(): Observable<LanguageDto[]> {
        const url = this.apiUrl.buildUrl('api/languages');

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new LanguageDto(
                            item.iso2Code,
                            item.englishName);
                    });
                })
                .catchError('Failed to load languages. Please reload');
    }
}