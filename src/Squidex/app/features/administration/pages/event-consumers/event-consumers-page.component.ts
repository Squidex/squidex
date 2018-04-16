/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

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
        this.eventConsumersState.load(false, true).onErrorResumeNext().subscribe();

        this.timerSubscription =
            Observable.timer(2000, 2000)
                .switchMap(x => this.eventConsumersState.load().onErrorResumeNext())
                .subscribe();
    }

    public reload() {
        this.eventConsumersState.load(true, true).onErrorResumeNext().subscribe();
    }

    public start(es: EventConsumerDto) {
        this.eventConsumersState.start(es).onErrorResumeNext().subscribe();
    }

    public stop(es: EventConsumerDto) {
        this.eventConsumersState.stop(es).onErrorResumeNext().subscribe();
    }

    public reset(es: EventConsumerDto) {
        this.eventConsumersState.reset(es).onErrorResumeNext().subscribe();
    }

    public trackByEventConsumer(index: number, es: EventConsumerDto) {
        return es.name;
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}