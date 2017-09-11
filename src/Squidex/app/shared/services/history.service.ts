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

import {
    ApiUrlConfig,
    HTTP,
    DateTime
} from 'framework';

export class HistoryEventDto {
    constructor(
        public readonly eventId: string,
        public readonly actor: string,
        public readonly message: string,
        public readonly version: number,
        public readonly created: DateTime
    ) {
    }
}

@Injectable()
export class HistoryService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getHistory(appName: string, channel: string): Observable<HistoryEventDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/history?channel=${channel}`);

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new HistoryEventDto(
                            item.eventId,
                            item.actor,
                            item.message,
                            item.version,
                            DateTime.parseISO_UTC(item.created));
                    });
                })
                .pretifyError('Failed to load history. Please reload.');
    }
}