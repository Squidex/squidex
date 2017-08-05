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
    DateTime,
    HTTP,
    Version
} from 'framework';

export class WebhookDto {
    constructor(
        public readonly id: string,
        public readonly schemaId: string,
        public readonly sharedSecret: string,
        public readonly url: string,
        public readonly totalSucceeded: number,
        public readonly totalFailed: number,
        public readonly totalTimedout: number,
        public readonly averageRequestTimeMs: number
    ) {
    }
}

export class WebhookEventDto {
    constructor(
        public readonly id: string,
        public readonly created: DateTime,
        public readonly nextAttempt: DateTime | null,
        public readonly eventName: string,
        public readonly requestUrl: string,
        public readonly lastDump: string,
        public readonly result: string,
        public readonly jobResult: string,
        public readonly numCalls: number
    ) {
    }
}

export class WebhookEventsDto {
    constructor(
        public readonly total: number,
        public readonly items: WebhookEventDto[]
    ) {
    }
}

export class CreateWebhookDto {
    constructor(
        public readonly url: string
    ) {
    }
}

@Injectable()
export class WebhooksService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getWebhooks(appName: string, version?: Version): Observable<WebhookDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks`);

        return HTTP.getVersioned(this.http, url, version)
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new WebhookDto(
                            item.id,
                            item.schemaId,
                            item.sharedSecret,
                            item.url,
                            item.totalSucceeded,
                            item.totalFailed,
                            item.totalTimedout,
                            item.averageRequestTimeMs);
                    });
                })
                .pretifyError('Failed to load webhooks. Please reload.');
    }

    public postWebhook(appName: string, schemaName: string, dto: CreateWebhookDto, version?: Version): Observable<WebhookDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/webhooks`);

        return HTTP.postVersioned(this.http, url, dto, version)
                .map(response => {
                    return new WebhookDto(
                        response.id,
                        response.schemaId,
                        response.sharedSecret,
                        dto.url,
                        0, 0, 0, 0);
                })
                .pretifyError('Failed to create webhook. Please reload.');
    }

    public deleteWebhook(appName: string, schemaName: string, id: string, version?: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/schemas/${schemaName}/webhooks/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .pretifyError('Failed to delete webhook. Please reload.');
    }

    public getEvents(appName: string, take: number, skip: number): Observable<WebhookEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks/events?take=${take}&skip=${skip}`);

        return HTTP.getVersioned(this.http, url)
                .map(response => {
                    const items: any[] = response.items;

                    return new WebhookEventsDto(response.total, items.map(item => {
                        return new WebhookEventDto(
                            item.id,
                            DateTime.parseISO_UTC(item.created),
                            item.nextAttempt ? DateTime.parseISO_UTC(item.nextAttempt) : null,
                            item.eventName,
                            item.requestUrl,
                            item.lastDump,
                            item.result,
                            item.jobResult,
                            item.numCalls);
                    }));
                })
                .pretifyError('Failed to load events. Please reload.');
    }

    public enqueueEvent(appName: string, id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks/events/${id}`);

        return HTTP.putVersioned(this.http, url, {})
                .pretifyError('Failed to enqueue webhook event. Please reload.');
    }
}