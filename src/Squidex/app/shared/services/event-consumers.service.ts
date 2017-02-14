/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig } from 'framework';
import { AuthService } from './auth.service';

export class EventConsumerDto {
    constructor(
        public readonly name: string,
        public readonly lastHandledEventNumber: number,
        public readonly isStopped: boolean,
        public readonly isResetting: boolean,
        public readonly error: string
    ) {
    }
}

@Injectable()
export class EventConsumersService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getEventConsumers(): Observable<EventConsumerDto[]> {
        const url = this.apiUrl.buildUrl('/api/event-consumers');

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new EventConsumerDto(
                            item.name,
                            item.lastHandledEventNumber,
                            item.isStopped,
                            item.isResetting,
                            item.error);
                    });
                })
                .catchError('Failed to load event consumers. Please reload.');
    }

    public startEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/start`);

        return this.authService.authPut(url, {})
                .map(response => response.json())
                .catchError('Failed to start event consumer. Please reload.');
    }

    public stopEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/stop`);

        return this.authService.authPut(url, {})
                .map(response => response.json())
                .catchError('Failed to stop event consumer. Please reload.');
    }

    public resetEventConsumer(name: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/event-consumers/${name}/reset`);

        return this.authService.authPut(url, {})
                .map(response => response.json())
                .catchError('Failed to reset event consumer. Please reload.');
    }
}