/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, hasAnyLink, pretifyError, Resource, ResourceLinks } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class EventConsumersDto {
    public readonly _links: ResourceLinks;

    constructor(
        public readonly items: ReadonlyArray<EventConsumerDto>, links?: ResourceLinks,
    ) {
        this._links = links || {};
    }
}

export class EventConsumerDto {
    public readonly _links: ResourceLinks;

    public readonly canStop: boolean;
    public readonly canStart: boolean;
    public readonly canReset: boolean;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly count: number,
        public readonly isStopped?: boolean,
        public readonly isResetting?: boolean,
        public readonly error?: string,
        public readonly position?: string,
    ) {
        this._links = links;

        this.canStop = hasAnyLink(links, 'stop');
        this.canStart = hasAnyLink(links, 'start');
        this.canReset = hasAnyLink(links, 'reset');
    }
}

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getEventConsumers(): Observable<EventConsumersDto> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.http.get<{ items: any[] } & Resource>(url).pipe(
            map(({ items, _links }) => {
                const eventConsumers = items.map(parseEventConsumer);

                return new EventConsumersDto(eventConsumers, _links);
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

function parseEventConsumer(response: any): EventConsumerDto {
    return new EventConsumerDto(
        response._links,
        response.name,
        response.count,
        response.isStopped,
        response.isResetting,
        response.error,
        response.position);
}
