/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    AppContext,
    EventConsumerDto,
    EventConsumersService,
    fadeAnimation,
    ImmutableArray,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class EventConsumersPageComponent implements OnDestroy, OnInit {
    private subscription: Subscription;

    public eventConsumerErrorDialog = new ModalView();
    public eventConsumerError = '';
    public eventConsumers = ImmutableArray.empty<EventConsumerDto>();

    constructor(public readonly ctx: AppContext,
        private readonly eventConsumersService: EventConsumersService
    ) {
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public ngOnInit() {
        this.load(false, true);

        this.subscription =
            Observable.timer(4000, 4000).subscribe(() => {
                this.load();
            });
    }

    public load(showInfo = false, showError = false) {
        this.eventConsumersService.getEventConsumers()
            .subscribe(dtos => {
                this.eventConsumers = ImmutableArray.of(dtos);

                if (showInfo) {
                    this.ctx.notifyInfo('Event Consumers reloaded.');
                }
            }, error => {
                if (showError) {
                    this.ctx.notifyError(error);
                }
            });
    }

    public start(consumer: EventConsumerDto) {
        this.eventConsumersService.startEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.start());
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public stop(consumer: EventConsumerDto) {
        this.eventConsumersService.stopEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.stop());
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public reset(consumer: EventConsumerDto) {
        this.eventConsumersService.resetEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', consumer.reset());
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}

