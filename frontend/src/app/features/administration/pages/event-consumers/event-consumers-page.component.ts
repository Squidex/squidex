/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { EventConsumerDto, EventConsumersState } from '@app/features/administration/internal';
import { DialogModel, Subscriptions } from '@app/shared';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
})
export class EventConsumersPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public eventConsumerErrorDialog = new DialogModel();
    public eventConsumerError?: string;

    constructor(
        public readonly eventConsumersState: EventConsumersState,
    ) {
    }

    public ngOnInit() {
        this.eventConsumersState.load();

        this.subscriptions.add(timer(1000, 1000).pipe(switchMap(() => this.eventConsumersState.load(false, true))));
    }

    public reload() {
        this.eventConsumersState.load(true, false);
    }

    public trackByEventConsumer(_index: number, es: EventConsumerDto) {
        return es.name;
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}
