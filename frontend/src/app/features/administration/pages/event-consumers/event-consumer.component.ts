/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TooltipDirective } from '@app/shared';
import { EventConsumerDto, EventConsumersState } from '../../internal';

@Component({
    standalone: true,
    selector: '[sqxEventConsumer]',
    styleUrls: ['./event-consumer.component.scss'],
    templateUrl: './event-consumer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TooltipDirective,
    ],
})
export class EventConsumerComponent {
    @Output()
    public failure = new EventEmitter();

    @Input('sqxEventConsumer')
    public eventConsumer!: EventConsumerDto;

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
