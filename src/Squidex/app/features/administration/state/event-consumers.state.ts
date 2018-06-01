/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    notify,
    State
} from '@app/shared';

import { EventConsumerDto, EventConsumersService } from './../services/event-consumers.service';

interface Snapshot {
    eventConsumers: ImmutableArray<EventConsumerDto>;

    isLoaded?: false;
}

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
            tap(dtos => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Event Consumers reloaded.');
                }

                this.next(s => {
                    const eventConsumers = ImmutableArray.of(dtos);

                    return { ...s, eventConsumers, isLoaded: true };
                });
            }),
            catchError(error => {
                if (silent) {
                    this.dialogs.notifyError(error);
                }

                return throwError(error);
            }));
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStart(eventConsumer.name).pipe(
            tap(() => {
                this.replaceEventConsumer(setStopped(eventConsumer, false));
            }),
            notify(this.dialogs));
    }

    public stop(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStop(eventConsumer.name).pipe(
            tap(() => {
                this.replaceEventConsumer(setStopped(eventConsumer, true));
            }),
            notify(this.dialogs));
    }

    public reset(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putReset(eventConsumer.name).pipe(
            tap(() => {
                this.replaceEventConsumer(reset(eventConsumer));
            }),
            notify(this.dialogs));
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