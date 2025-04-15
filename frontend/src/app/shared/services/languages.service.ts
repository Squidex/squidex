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
import { LanguageDto } from './../model';

@Injectable({
    providedIn: 'root',
})
export class LanguagesService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getLanguages(): Observable<ReadonlyArray<LanguageDto>> {
        const url = this.apiUrl.buildUrl('api/languages');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(LanguageDto.fromJSON);
            }),
            pretifyError('i18n:languages.loadFailed'));
    }
}
