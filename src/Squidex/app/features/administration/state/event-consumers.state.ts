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
    eventConsumers: ImmutableArray<EventConsumerDto>;

    // Indicates if event consumers are loaded.
    isLoaded?: boolean;
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

    public load(isReload = false, silent = false): Observable<EventConsumerDto[]> {
        if (!isReload) {
            this.resetState();
        }

        const stream = this.eventConsumersService.getEventConsumers().pipe(share());

        stream.subscribe(dtos => {
            if (isReload && !silent) {
                this.dialogs.notifyInfo('Event Consumers reloaded.');
            }

            this.next(s => {
                const eventConsumers = ImmutableArray.of(dtos);

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
        const stream = this.eventConsumersService.putStart(eventConsumer.name).pipe(share());

        stream.subscribe(() => {
            this.replaceEventConsumer(setStopped(eventConsumer, false));
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
    }

    public stop(eventConsumer: EventConsumerDto): Observable<any> {
        const stream = this.eventConsumersService.putStop(eventConsumer.name).pipe(share());

        stream.subscribe(() => {
            this.replaceEventConsumer(setStopped(eventConsumer, true));
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
    }

    public reset(eventConsumer: EventConsumerDto): Observable<any> {
        const stream = this.eventConsumersService.putReset(eventConsumer.name).pipe(share());

        stream.subscribe(() => {
            this.replaceEventConsumer(reset(eventConsumer));
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
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