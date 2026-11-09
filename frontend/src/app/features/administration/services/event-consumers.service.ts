/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, EventConsumerDto, EventConsumersDto, IResourceDto, pretifyError } from '@app/shared';

@Service({ autoProvided: false })
export class EventConsumersService {
    private readonly apiUrl = inject(ApiUrlConfig);
    private readonly http = inject(HttpClient);

    public getEventConsumers(): Observable<EventConsumersDto> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.http.get<any>(url).pipe(
            map(body => {
                return EventConsumersDto.fromJSON(body);
            }),
            pretifyError('i18n:eventConsumers.loadFailed'));
    }

    public putStart(eventConsumer: IResourceDto): Observable<EventConsumerDto> {
        const link = eventConsumer._links['start'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return EventConsumerDto.fromJSON(body);
            }),
            pretifyError('i18n:eventConsumers.startFailed'));
    }

    public putStop(eventConsumer: IResourceDto): Observable<EventConsumerDto> {
        const link = eventConsumer._links['stop'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return EventConsumerDto.fromJSON(body);
            }),
            pretifyError('i18n:eventConsumers.stopFailed'));
    }

    public putReset(eventConsumer: IResourceDto): Observable<EventConsumerDto> {
        const link = eventConsumer._links['reset'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return EventConsumerDto.fromJSON(body);
            }),
            pretifyError('i18n:eventConsumers.resetFailed'));
    }
}
