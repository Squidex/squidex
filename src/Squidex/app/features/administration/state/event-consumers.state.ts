/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State
} from '@app/shared';

import { EventConsumerDto, EventConsumersService } from './../services/event-consumers.service';

interface Snapshot {
    // The list of event consumers.
    eventConsumers: EventConsumersList;

    // Indicates if event consumers are loaded.
    isLoaded?: boolean;
}

type EventConsumersList = ImmutableArray<EventConsumerDto>;

@Injectable()
export class EventConsumersState extends State<Snapshot> {
    public eventConsumers =
        this.project(x => x.eventConsumers);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    constructor(
        private readonly dialogs: DialogService,
        private readonly eventConsumersService: EventConsumersService
    ) {
        super({ eventConsumers: ImmutableArray.empty() });
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.eventConsumersService.getEventConsumers().pipe(
            tap(({ items }) => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Event Consumers reloaded.');
                }

                const eventConsumers = ImmutableArray.of(items);

                this.next(s => {
                    return { ...s, eventConsumers, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStart(eventConsumer).pipe(
            tap(updated => {
                this.replaceEventConsumer(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public stop(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        return this.eventConsumersService.putStop(eventConsumer).pipe(
            tap(updated => {
                this.replaceEventConsumer(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public reset(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        return this.eventConsumersService.putReset(eventConsumer).pipe(
            tap(updated => {
                this.replaceEventConsumer(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceEventConsumer(eventConsumer: EventConsumerDto) {
        this.next(s => {
            const eventConsumers = s.eventConsumers.replaceBy('name', eventConsumer);

            return { ...s, eventConsumers };
        });
    }
}