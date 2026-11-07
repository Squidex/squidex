/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
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
    private readonly eventConsumersState = inject(EventConsumersState);

    /** Emits when the user clicks the error icon to see the failure details. */
    public readonly failure = output();

    /** The event consumer to show in the row. */
    public readonly eventConsumer = input.required<EventConsumerDto>({ alias: 'sqxEventConsumer' });

    protected start() {
        this.eventConsumersState.start(this.eventConsumer());
    }

    protected stop() {
        this.eventConsumersState.stop(this.eventConsumer());
    }

    protected reset() {
        this.eventConsumersState.reset(this.eventConsumer());
    }
}
