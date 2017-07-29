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
                .switchMap(() => this.eventConsumersService.getEventConsumers())
                .subscribe(dtos => {
                    this.eventConsumers = ImmutableArray.of(dtos);
                });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public start(consumer: EventConsumerDto) {
        this.eventConsumersService.startEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.start());
            }, error => {
                this.notifyError(error);
            });
    }

    public stop(consumer: EventConsumerDto) {
        this.eventConsumersService.stopEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.stop());
            }, error => {
                this.notifyError(error);
            });
    }

    public reset(consumer: EventConsumerDto) {
        this.eventConsumersService.resetEventConsumer(name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.reset());
            }, error => {
                this.notifyError(error);
            });
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}

