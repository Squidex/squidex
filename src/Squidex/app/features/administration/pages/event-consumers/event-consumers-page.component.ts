/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    ComponentBase,
    EventConsumerDto,
    EventConsumersService,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    NotificationService
} from 'shared';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class EventConsumersPageComponent extends ComponentBase implements OnInit, OnDestroy {
    private subscription: Subscription;

    public eventConsumerErrorDialog = new ModalView();
    public eventConsumerError = '';
    public eventConsumers = ImmutableArray.empty<EventConsumerDto>();

    constructor(notifications: NotificationService,
        private readonly eventConsumersService: EventConsumersService
    ) {
        super(notifications);
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
                this.eventConsumers =
                    this.eventConsumers.replaceAll(
                        e => e.name === name,
                        e => new EventConsumerDto(name, false, e.isResetting, e.error, e.position));
            }, error => {
                this.notifyError(error);
            });
    }

    public stop(name: string) {
        this.eventConsumersService.stopEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers =
                    this.eventConsumers.replaceAll(
                        e => e.name === name,
                        e => new EventConsumerDto(name, true, e.isResetting, e.error, e.position));
            }, error => {
                this.notifyError(error);
            });
    }

    public reset(name: string) {
        this.eventConsumersService.resetEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers =
                    this.eventConsumers.replaceAll(
                        e => e.name === name,
                        e => new EventConsumerDto(name, e.isStopped, true, e.error, e.position));
            }, error => {
                this.notifyError(error);
            });
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}

