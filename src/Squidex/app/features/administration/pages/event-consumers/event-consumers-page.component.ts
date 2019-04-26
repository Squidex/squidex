/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import { DialogModel, ResourceOwner } from '@app/shared';

import { EventConsumerDto, EventConsumersState } from '../../declarations';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html'
})
export class EventConsumersPageComponent extends ResourceOwner implements OnInit {
    public eventConsumerErrorDialog = new DialogModel();
    public eventConsumerError?: string;

    constructor(
        public readonly eventConsumersState: EventConsumersState
    ) {
        super();
    }

    public ngOnInit() {
        this.eventConsumersState.load().pipe(onErrorResumeNext()).subscribe();

        this.own(timer(5000, 5000).pipe(switchMap(() => this.eventConsumersState.load(true, true)), onErrorResumeNext()));
    }

    public reload() {
        this.eventConsumersState.load(true, false).pipe(onErrorResumeNext()).subscribe();
    }

    public start(es: EventConsumerDto) {
        this.eventConsumersState.start(es).pipe(onErrorResumeNext()).subscribe();
    }

    public stop(es: EventConsumerDto) {
        this.eventConsumersState.stop(es).pipe(onErrorResumeNext()).subscribe();
    }

    public reset(es: EventConsumerDto) {
        this.eventConsumersState.reset(es).pipe(onErrorResumeNext()).subscribe();
    }

    public trackByEventConsumer(index: number, es: EventConsumerDto) {
        return es.name;
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}