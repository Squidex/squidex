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

import { ApiUrlConfig, HTTP } from 'framework';

export class EventConsumerDto {
    constructor(
        public readonly name: string,
        public readonly isStopped: boolean,
        public readonly isResetting: boolean,
        public readonly error: string,
        public readonly position: string
    ) {
    }

    public start(): EventConsumerDto {
        return new EventConsumerDto(this.name, false, false, this.error, this.position);
    }

    public stop(): EventConsumerDto {
        return new EventConsumerDto(this.name, true, false, this.error, this.position);
    }

    public reset(): EventConsumerDto {
        return new EventConsumerDto(this.name, this.isStopped, true, this.error, this.position);
    }
}

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getEventConsumers(): Observable<EventConsumerDto[]> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return HTTP.getVersioned<any>(this.http, url)
                .map(response => {
                    const body = response.payload.body;

                    const items: any[] = body;

                    return items.map(item => {
                        return new EventConsumerDto(
                            item.name,
                            item.isStopped,
                            item.isResetting,
                            item.error,
                            item.position);
                    });
                })
                .pretifyError('Failed to load event consumers. Please reload.');
    }

    public startEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/start`);

        return HTTP.putVersioned(this.http, url, {})
                .pretifyError('Failed to start event consumer. Please reload.');
    }

    public stopEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/stop`);

        return HTTP.putVersioned(this.http, url, {})
                .pretifyError('Failed to stop event consumer. Please reload.');
    }

    public resetEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/reset`);

        return HTTP.putVersioned(this.http, url, {})
                .pretifyError('Failed to reset event consumer. Please reload.');
    }
}