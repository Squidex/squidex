/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { EventConsumerDto, TooltipDirective, TranslatePipe } from '@app/shared';
import { EventConsumersState } from '../../internal';

@Component({
    selector: '[sqxEventConsumer]',
    styleUrls: ['./event-consumer.component.scss'],
    templateUrl: './event-consumer.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
        TooltipDirective,
    ],
})
export class EventConsumerComponent {
    /** Emits when the user clicks the error icon to see the failure details. */
    public readonly failure = output();

    /** The event consumer to show in the row. */
    public readonly eventConsumer = input.required<EventConsumerDto>({ alias: 'sqxEventConsumer' });

    constructor(
        private readonly eventConsumersState: EventConsumersState,
    ) {
    }

    public start() {
        this.eventConsumersState.start(this.eventConsumer());
    }

    public stop() {
        this.eventConsumersState.stop(this.eventConsumer());
    }

    public reset() {
        this.eventConsumersState.reset(this.eventConsumer());
    }
}
