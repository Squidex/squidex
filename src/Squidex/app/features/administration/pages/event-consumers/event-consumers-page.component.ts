/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription, timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import { ModalView } from '@app/shared';

import { EventConsumerDto } from './../../services/event-consumers.service';
import { EventConsumersState } from './../../state/event-consumers.state';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html'
})
export class EventConsumersPageComponent implements OnDestroy, OnInit {
    private timerSubscription: Subscription;

    public eventConsumerErrorDialog = new ModalView();
    public eventConsumerError = '';

    constructor(
        public readonly eventConsumersState: EventConsumersState
    ) {
    }

    public ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.eventConsumersState.load(false, true).pipe(onErrorResumeNext()).subscribe();

        this.timerSubscription =
            timer(2000, 2000).pipe(
                    switchMap(x => this.eventConsumersState.load(true, true)), onErrorResumeNext())
                .subscribe();
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