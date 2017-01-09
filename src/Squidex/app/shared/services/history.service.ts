/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig, DateTime } from 'framework';

import { AuthService } from './auth.service';

export class HistoryEventDto {
    constructor(
        public readonly eventId: string,
        public readonly actor: string,
        public readonly message: string,
        public readonly created: DateTime
    ) {
    }
}

@Injectable()
export class HistoryService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getHistory(appName: string, channel: string): Observable<HistoryEventDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/history?channel=${channel}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new HistoryEventDto(
                            item.eventId,
                            item.actor,
                            item.message,
                            DateTime.parseISO_UTC(item.created));
                    });
                })
                .catchError('Failed to load history. Please reload');
    }
}