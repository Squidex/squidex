/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

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
        this.changes.pipe(map(x => x.eventConsumers),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

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
            tap(payload => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Event Consumers reloaded.');
                }

                const eventConsumers = ImmutableArray.of(payload);

                this.next(s => {
                    return { ...s, eventConsumers, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStart(eventConsumer.name).pipe(
            map(() => setStopped(eventConsumer, false)),
            tap(updated => {
                this.replaceEventConsumer(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public stop(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        return this.eventConsumersService.putStop(eventConsumer.name).pipe(
            map(() => setStopped(eventConsumer, true)),
            tap(updated => {
                this.replaceEventConsumer(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public reset(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        return this.eventConsumersService.putReset(eventConsumer.name).pipe(
            map(() => reset(eventConsumer)),
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

const setStopped = (eventConsumer: EventConsumerDto, isStopped: boolean) =>
    eventConsumer.with({ isStopped });

const reset = (eventConsumer: EventConsumerDto) =>
    eventConsumer.with({ isResetting: true });