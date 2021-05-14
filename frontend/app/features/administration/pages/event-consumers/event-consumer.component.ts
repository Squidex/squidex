/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { EventConsumerDto, EventConsumersState } from '@app/features/administration/internal';

@Component({
    selector: '[sqxEventConsumer]',
    styleUrls: ['./event-consumer.component.scss'],
    templateUrl: './event-consumer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventConsumerComponent {
    @Output()
    public error = new EventEmitter();

    @Input('sqxEventConsumer')
    public eventConsumer: EventConsumerDto;

    constructor(
        private readonly eventConsumersState: EventConsumersState,
    ) {
    }

    public start() {
        this.eventConsumersState.start(this.eventConsumer);
    }

    public stop() {
        this.eventConsumersState.stop(this.eventConsumer);
    }

    public reset() {
        this.eventConsumersState.reset(this.eventConsumer);
    }
}
