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
    array,
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

        const stream =
            this.eventConsumersService.getEventConsumers().pipe(
                map(dtos => array(dtos)), share());

        stream.subscribe(eventConsumers => {
            if (isReload && !silent) {
                this.dialogs.notifyInfo('Event Consumers reloaded.');
            }

            this.next(s => {
                return { ...s, eventConsumers, isLoaded: true };
            });

        }, error => {
            if (!silent) {
                this.dialogs.notifyError(error);
            }
        });

        return stream;
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        const stream =
            this.eventConsumersService.putStart(eventConsumer.name).pipe(
                map(() => setStopped(eventConsumer, false), share()));

        this.updateState(stream);

        return stream;
    }

    public stop(eventConsumer: EventConsumerDto): Observable<EventConsumerDto> {
        const stream =
            this.eventConsumersService.putStop(eventConsumer.name).pipe(
                map(() => setStopped(eventConsumer, true), share()));

        this.updateState(stream);

        return stream;
    }

    public reset(eventConsumer: EventConsumerDto): Observable<any> {
        const stream =
            this.eventConsumersService.putReset(eventConsumer.name).pipe(
                map(() => reset(eventConsumer), share()));

        this.updateState(stream);

        return stream;
    }

    private updateState(stream: Observable<EventConsumerDto>) {
        stream.subscribe(updated => {
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