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
    pretifyError,
    Resource,
    ResourceLinks,
    withLinks
} from '@app/shared';

export class EventConsumersDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly items: EventConsumerDto[]
    ) {
    }
}

export class EventConsumerDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly name: string,
        public readonly isStopped?: boolean,
        public readonly isResetting?: boolean,
        public readonly error?: string,
        public readonly position?: string
    ) {
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
                map(body => {
                    const eventConsumers = body.items.map(item => parseEventConsumer(item));

                    return withLinks(new EventConsumersDto(eventConsumers), body);
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
    return withLinks(
        new EventConsumerDto(
            response.name,
            response.isStopped,
            response.isResetting,
            response.error,
            response.position),
        response);
}
