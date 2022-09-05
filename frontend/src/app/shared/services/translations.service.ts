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

export class TranslationDto {
    constructor(
        public readonly result: string,
        public readonly text: string,
    ) {
    }
}

export type TranslateDto = Readonly<{
    // The text to translate.
    text: string;

    // The source language.
    sourceLanguage: string;

    // The target language.
    targetLanguage: string;
 }>;

@Injectable()
export class TranslationsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public translate(appName: string, request: TranslateDto): Observable<TranslationDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/translations`);

        return this.http.post<any>(url, request).pipe(
            map(body => {
                return parseTranslation(body);
            }),
            pretifyError('i18n:translate.translateFailed'));
    }
}

function parseTranslation(body: any): TranslationDto {
    return new TranslationDto(body.result, body.text);
}

