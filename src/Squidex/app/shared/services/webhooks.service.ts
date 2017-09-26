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
    AnalyticsService,
    ApiUrlConfig,
    DateTime,
    HTTP,
    Version,
    Versioned
} from 'framework';

export class WebhookDto {
    constructor(
        public readonly id: string,
        public readonly sharedSecret: string,
        public readonly createdBy: string,
        public readonly lastModifiedBy: string,
        public readonly created: DateTime,
        public readonly lastModified: DateTime,
        public readonly version: Version,
        public readonly schemas: WebhookSchemaDto[],
        public readonly url: string,
        public readonly totalSucceeded: number,
        public readonly totalFailed: number,
        public readonly totalTimedout: number,
        public readonly averageRequestTimeMs: number
    ) {
    }

    public update(update: UpdateWebhookDto, user: string, version: Version, now?: DateTime): WebhookDto {
        return new WebhookDto(
            this.id,
            this.sharedSecret,
            this.createdBy, user,
            this.created, now || DateTime.now(),
            version,
            update.schemas,
            update.url,
            this.totalSucceeded,
            this.totalFailed,
            this.totalTimedout,
            this.averageRequestTimeMs);
    }
}

export class WebhookSchemaDto {
    constructor(
        public readonly schemaId: string,
        public readonly sendCreate: boolean,
        public readonly sendUpdate: boolean,
        public readonly sendDelete: boolean,
        public readonly sendPublish: boolean
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
        public readonly url: string,
        public readonly schemas: WebhookSchemaDto[]
    ) {
    }
}

export class UpdateWebhookDto {
    constructor(
        public readonly url: string,
        public readonly schemas: WebhookSchemaDto[]
    ) {
    }
}

@Injectable()
export class WebhooksService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getWebhooks(appName: string): Observable<WebhookDto[]> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const items: any[] = response.payload.body;

                    return items.map(item => {
                        const schemas = item.schemas.map((schema: any) =>
                            new WebhookSchemaDto(
                                schema.schemaId,
                                schema.sendCreate,
                                schema.sendUpdate,
                                schema.sendDelete,
                                schema.sendPublish));

                        return new WebhookDto(
                            item.id,
                            item.sharedSecret,
                            item.createdBy,
                            item.lastModifiedBy,
                            DateTime.parseISO_UTC(item.created),
                            DateTime.parseISO_UTC(item.lastModified),
                            new Version(item.version.toString()),
                            schemas,
                            item.url,
                            item.totalSucceeded,
                            item.totalFailed,
                            item.totalTimedout,
                            item.averageRequestTimeMs);
                    });
                })
                .pretifyError('Failed to load webhooks. Please reload.');
    }

    public postWebhook(appName: string, dto: CreateWebhookDto, user: string, now: DateTime): Observable<WebhookDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks`);

        return HTTP.postVersioned<any>(this.http, url, dto)
                .map(response => {
                    const body = response.payload.body;

                    return new WebhookDto(
                        body.id,
                        body.sharedSecret,
                        user,
                        user,
                        now,
                        now,
                        response.version,
                        dto.schemas,
                        dto.url,
                        0, 0, 0, 0);
                })
                .do(() => {
                    this.analytics.trackEvent('Webhook', 'Created', appName);
                })
                .pretifyError('Failed to create webhook. Please reload.');
    }

    public putWebhook(appName: string, id: string, dto: UpdateWebhookDto, version: Version): Observable<Versioned<any>> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks/${id}`);

        return HTTP.putVersioned(this.http, url, dto, version)
                .do(() => {
                    this.analytics.trackEvent('Webhook', 'Updated', appName);
                })
                .pretifyError('Failed to update webhook. Please reload.');
    }

    public deleteWebhook(appName: string, id: string, version: Version): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks/${id}`);

        return HTTP.deleteVersioned(this.http, url, version)
                .do(() => {
                    this.analytics.trackEvent('Webhook', 'Deleted', appName);
                })
                .pretifyError('Failed to delete webhook. Please reload.');
    }

    public getEvents(appName: string, take: number, skip: number): Observable<WebhookEventsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/webhooks/events?take=${take}&skip=${skip}`);

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body.items;

                    return new WebhookEventsDto(body.total, items.map(item => {
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
                .do(() => {
                    this.analytics.trackEvent('Webhook', 'EventEnqueued', appName);
                })
                .pretifyError('Failed to enqueue webhook event. Please reload.');
    }
}