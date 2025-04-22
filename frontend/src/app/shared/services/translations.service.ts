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
import { TranslateDto, TranslationDto } from './../model';
import { AuthService } from './auth.service';

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

    public translate(appName: string, dto: TranslateDto): Observable<TranslationDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/translations`);

        return this.http.post<any>(url, dto.toJSON()).pipe(
            map(body => {
                return TranslationDto.fromJSON(body);
            }),
            pretifyError('i18n:translate.translateFailed'));
    }

    public ask(appName: string, dto: AskDto): Observable<ChatEventDto> {
        const token = this.authService.user!.accessToken;

        const url = this.apiUrl.buildUrl(`api/apps/${appName}/ask${StringHelper.buildQuery({ ...dto, access_token: token })}`);

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