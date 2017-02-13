/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    EventConsumerDto,
    EventConsumersService,
    ImmutableArray
} from 'shared';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html'
})
export class EventConsumersPage implements OnInit, OnDestroy {
    private subscription: Subscription;

    public eventConsumers = ImmutableArray.empty<EventConsumerDto>();

    constructor(
        private readonly eventConsumersService: EventConsumersService
    ) {
    }

    public ngOnInit() {
        this.subscription =
            Observable.timer(0, 4000)
                .switchMap(_ => this.eventConsumersService.getEventConsumers())
                .subscribe(dtos => {
                    this.eventConsumers = ImmutableArray.of(dtos);
                });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public start(name: string) {
        this.eventConsumersService.startEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.map(e => {
                    if (e.name === name) {
                        return new EventConsumerDto(name, e.lastHandledEventNumber, false, e.isResetting);
                    } else {
                        return e;
                    }
                });
            });
    }

    public stop(name: string) {
        this.eventConsumersService.stopEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.map(e => {
                    if (e.name === name) {
                        return new EventConsumerDto(name, e.lastHandledEventNumber, true, e.isResetting);
                    } else {
                        return e;
                    }
                });
            });
    }

    public reset(name: string) {
        this.eventConsumersService.resetEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.map(e => {
                    if (e.name === name) {
                        return new EventConsumerDto(name, e.lastHandledEventNumber, e.isStopped, true);
                    } else {
                        return e;
                    }
                });
            });
    }
}

