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
import { ApiUrlConfig, pretifyError, StringHelper } from '@app/framework';
import { AuthService } from './auth.service';

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

export type AskDto = Readonly<{
    // Optional conversation ID.
    conversationId?: string;

    // The configuration.
    configuration?: string;

    // The question to ask.
    prompt?: string;
}>;

export interface ChatChunkDto {
    type: 'Chunk';

    // The content of the chunk.
    content: string;
}

export interface ChatToolStartDto {
    type: 'ToolStart';

    // The tool that has been started.
    tool: string;
}

export interface ChatToolEndDto {
    type: 'ToolEnd';

    // The tool that has been finished.
    tool: string;
}

export type ChatEventDto = ChatChunkDto | ChatToolStartDto | ChatToolEndDto;


@Injectable({
    providedIn: 'root',
})
export class TranslationsService {
    constructor(
        private readonly http: HttpClient,
        private readonly authService: AuthService,
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

    public ask(appName: string, request: AskDto): Observable<ChatEventDto> {
        const token = this.authService.user!.accessToken;

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ask${StringHelper.buildQuery({ ...request, access_token: token })}`);

        return new Observable<ChatEventDto>((subscriber) => {
            const source = new EventSource(url);

            source.addEventListener('message', (event) => {
                if (!event) {
                    source.close();

                    subscriber.complete();
                } else {
                    subscriber.next(JSON.parse(event.data));
                }
            });

            source.addEventListener('error', (event) => {

            const data = (event as any)['data'];
            try {
                if (data) {
                    try {
                        subscriber.error(JSON.parse(data).message);
                    } finally {
                        subscriber.error(data);
                    }
                }
            } finally {
                subscriber.complete();
                source.close();
            }
            });

            return () => {
                source.close();
            };
        });
    }
}

function parseTranslation(body: any): TranslationDto {
    return new TranslationDto(body.result, body.text);
}

