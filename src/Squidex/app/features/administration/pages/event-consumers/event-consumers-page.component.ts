/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    fadeAnimation,
    DialogService,
    ImmutableArray,
    ModalView
} from '@app/shared';

import { EventConsumerDto, EventConsumersService } from './../../services/event-consumers.service';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class EventConsumersPageComponent implements OnDestroy, OnInit {
    private subscription: Subscription;

    public eventConsumerErrorDialog = new ModalView();
    public eventConsumerError = '';
    public eventConsumers = ImmutableArray.empty<EventConsumerDto>();

    constructor(
        private readonly dialogs: DialogService,
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
                    this.dialogs.notifyInfo('Event Consumers reloaded.');
                }
            }, error => {
                if (showError) {
                    this.dialogs.notifyError(error);
                }
            });
    }

    public start(consumer: EventConsumerDto) {
        this.eventConsumersService.startEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', start(consumer));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public stop(consumer: EventConsumerDto) {
        this.eventConsumersService.stopEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', stop(consumer));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public reset(consumer: EventConsumerDto) {
        this.eventConsumersService.resetEventConsumer(consumer.name)
            .subscribe(() => {
                this.eventConsumers = this.eventConsumers.replaceBy('name', reset(consumer));
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}



function start(es: EventConsumerDto) {
    return new EventConsumerDto(es.name, false, false, es.error, es.position);
}

function stop(es: EventConsumerDto) {
    return new EventConsumerDto(es.name, true, false, es.error, es.position);
}

function reset(es: EventConsumerDto) {
    return new EventConsumerDto(es.name, es.isStopped, true, es.error, es.position);
}