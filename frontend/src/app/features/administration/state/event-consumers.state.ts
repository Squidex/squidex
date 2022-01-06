/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, shareSubscribed, State } from '@app/shared';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { EventConsumerDto, EventConsumersService } from './../services/event-consumers.service';

interface Snapshot {
    // The list of event consumers.
    eventConsumers: EventConsumersList;

    // Indicates if event consumers are loaded.
    isLoaded?: boolean;

    // Indicates if event consumers are loading.
    isLoading?: boolean;
}

type EventConsumersList = ReadonlyArray<EventConsumerDto>;

@Injectable()
export class EventConsumersState extends State<Snapshot> {
    public eventConsumers =
        this.project(x => x.eventConsumers);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    constructor(
        private readonly dialogs: DialogService,
        private readonly eventConsumersService: EventConsumersService,
    ) {
        super({ eventConsumers: [] }, 'EventConsumers');
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (isReload && !silent) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, silent);
    }

    private loadInternal(isReload: boolean, silent: boolean): Observable<any> {
        if (!silent) {
            this.next({ isLoading: true }, 'Loading Started');
        }

        return this.eventConsumersService.getEventConsumers().pipe(
            tap(({ items: eventConsumers }) => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('i18n:eventConsumers.reloaded');
                }

                this.next({
                    eventConsumers,
                    isLoaded: true,
                    isLoading: false,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
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
            const eventConsumers = s.eventConsumers.replacedBy('name', eventConsumer);

            return { ...s, eventConsumers };
        }, 'Updated');
    }
}
