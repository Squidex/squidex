/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
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

        const http$ =
            this.eventConsumersService.getEventConsumers().pipe(
                share());

        http$.subscribe(response => {
            if (isReload && !silent) {
                this.dialogs.notifyInfo('Event Consumers reloaded.');
            }

            const eventConsumers = ImmutableArray.of(response);

            this.next(s => {
                return { ...s, eventConsumers, isLoaded: true };
            });

        }, error => {
            if (!silent) {
                this.dialogs.notifyError(error);
            }
        });

        return http$;
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        const http$ =
            this.eventConsumersService.putStart(eventConsumer.name).pipe(
                map(() => setStopped(eventConsumer, false), share()));

        this.updateState(http$);

        return http$;
    }

    public stop(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        const http$ =
            this.eventConsumersService.putStop(eventConsumer.name).pipe(
                map(() => setStopped(eventConsumer, true), share()));

        this.updateState(http$);

        return http$;
    }

    public reset(eventConsumer: EventConsumerDto): Observable<any> {
        const http$ =
            this.eventConsumersService.putReset(eventConsumer.name).pipe(
                map(() => reset(eventConsumer), share()));

        this.updateState(http$);

        return http$;
    }

    private updateState(http$: Observable<EventConsumerDto>) {
        http$.subscribe(updated => {
            this.replaceEventConsumer(updated);
        }, error => {
            this.dialogs.notifyError(error);
        });
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