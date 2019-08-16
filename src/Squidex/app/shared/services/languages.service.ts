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

import { ApiUrlConfig, pretifyError } from '@app/framework';

export class LanguageDto {
    constructor(
        public readonly iso2Code: string,
        public readonly englishName: string
    ) {
    }
}

@Injectable()
export class LanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getLanguages(): Observable<LanguageDto[]> {
        const url = this.apiUrl.buildUrl('api/languages');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const languages = body.map(item =>
                    new LanguageDto(
                        item.iso2Code,
                        item.englishName));

                return languages;
            }),
            pretifyError('Failed to load languages. Please reload.'));
    }
}