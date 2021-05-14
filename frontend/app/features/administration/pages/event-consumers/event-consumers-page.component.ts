/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { EventConsumerDto, EventConsumersState } from '@app/features/administration/internal';
import { DialogModel, ResourceOwner } from '@app/shared';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
})
export class EventConsumersPageComponent extends ResourceOwner implements OnInit {
    public eventConsumerErrorDialog = new DialogModel();
    public eventConsumerError?: string;

    constructor(
        public readonly eventConsumersState: EventConsumersState,
    ) {
        super();
    }

    public ngOnInit() {
        this.eventConsumersState.load();

        this.own(timer(5000, 5000).pipe(switchMap(() => this.eventConsumersState.load(false, true))));
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
