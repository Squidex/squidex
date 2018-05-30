/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DialogService,
    ImmutableArray,
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
        this.changes.map(x => x.eventConsumers)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

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

        return this.eventConsumersService.getEventConsumers()
            .do(dtos => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Event Consumers reloaded.');
                }

                this.next(s => {
                    const eventConsumers = ImmutableArray.of(dtos);

                    return { ...s, eventConsumers, isLoaded: true };
                });
            })
            .catch(error => {
                if (silent) {
                    this.dialogs.notifyError(error);
                }

                return Observable.throw(error);
            });
    }

    public start(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStart(eventConsumer.name)
            .do(() => {
                this.replaceEventConsumer(setStopped(eventConsumer, false));
            })
            .notify(this.dialogs);
    }

    public stop(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putStop(eventConsumer.name)
            .do(() => {
                this.replaceEventConsumer(setStopped(eventConsumer, true));
            })
            .notify(this.dialogs);
    }

    public reset(eventConsumer: EventConsumerDto): Observable<any> {
        return this.eventConsumersService.putReset(eventConsumer.name)
            .do(() => {
                this.replaceEventConsumer(reset(eventConsumer));
            })
            .notify(this.dialogs);
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