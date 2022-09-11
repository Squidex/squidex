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
import { ApiUrlConfig, hasAnyLink, pretifyError, Resource, ResourceLinks } from '@app/shared';

export class EventConsumerDto implements Resource {
    public readonly _links: ResourceLinks;

    public readonly canReset: boolean;
    public readonly canStart: boolean;
    public readonly canStop: boolean;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly count: number,
        public readonly isStopped?: boolean,
        public readonly isResetting?: boolean,
        public readonly error?: string,
        public readonly position?: string,
    ) {
        this._links = links;

        this.canReset = hasAnyLink(links, 'reset');
        this.canStart = hasAnyLink(links, 'start');
        this.canStop = hasAnyLink(links, 'stop');
    }
}

export type EventConsumersDto = Readonly<{
    // The list of event consumers.
    items: ReadonlyArray<EventConsumerDto>;
}>;

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getEventConsumers(): Observable<EventConsumersDto> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseEventConsumers(body);
            }),
            pretifyError('i18n:eventConsumers.loadFailed'));
    }

    public putStart(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['start'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('i18n:eventConsumers.startFailed'));
    }

    public putStop(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['stop'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('i18n:eventConsumers.stopFailed'));
    }

    public putReset(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['reset'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('i18n:eventConsumers.resetFailed'));
    }
}

function parseEventConsumers(response: { items: any[] } & Resource): EventConsumersDto {
    const { items: list } = response;
    const items = list.map(parseEventConsumer);

    return { items };
}

function parseEventConsumer(response: any): EventConsumerDto {
    return new EventConsumerDto(response._links,
        response.name,
        response.count,
        response.isStopped,
        response.isResetting,
        response.error,
        response.position);
}
