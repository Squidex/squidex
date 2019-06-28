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

import {
    ApiUrlConfig,
    hasAnyLink,
    pretifyError,
    Resource,
    ResourceLinks
} from '@app/shared';

export class EventConsumersDto {
    public readonly _links: ResourceLinks;

    constructor(
        public readonly items: EventConsumerDto[], links?: ResourceLinks
    ) {
        this._links = links || {};
    }
}

export class EventConsumerDto {
    public readonly _links: ResourceLinks;

    public readonly canStop: boolean;
    public readonly canStart: boolean;
    public readonly canRestart: boolean;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly isStopped?: boolean,
        public readonly isResetting?: boolean,
        public readonly error?: string,
        public readonly position?: string
    ) {
        this._links = links;

        this.canStop = hasAnyLink(links, 'stop');
        this.canStart = hasAnyLink(links, 'start');
        this.canRestart = hasAnyLink(links, 'canReset');
    }
}

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getEventConsumers(): Observable<EventConsumersDto> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.http.get<{ items: any[] } & Resource>(url).pipe(
            map(({ items, _links }) => {
                const eventConsumers = items.map(item => parseEventConsumer(item));

                return new EventConsumersDto(eventConsumers, _links);
            }),
            pretifyError('Failed to load event consumers. Please reload.'));
    }

    public putStart(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['start'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('Failed to start event consumer. Please reload.'));
    }

    public putStop(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['stop'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('Failed to stop event consumer. Please reload.'));
    }

    public putReset(eventConsumer: Resource): Observable<EventConsumerDto> {
        const link = eventConsumer._links['reset'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseEventConsumer(body);
            }),
            pretifyError('Failed to reset event consumer. Please reload.'));
    }
}

function parseEventConsumer(response: any): EventConsumerDto {
    return new EventConsumerDto(
        response._links,
        response.name,
        response.isStopped,
        response.isResetting,
        response.error,
        response.position);
}
