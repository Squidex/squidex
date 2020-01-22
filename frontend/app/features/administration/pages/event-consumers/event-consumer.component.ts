/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { EventConsumerDto, EventConsumersState } from '@app/features/administration/internal';

@Component({
    selector: '[sqxEventConsumer]',
    template: `
        <tr [class.faulted]="eventConsumer.error && eventConsumer.error?.length > 0">
            <td class="cell-auto">
                <span class="truncate">
                    <i class="faulted-icon icon icon-bug" (click)="error.emit()" [class.hidden]="!eventConsumer.error || eventConsumer.error?.length === 0"></i>

                    {{eventConsumer.name}}
                </span>
            </td>
            <td class="cell-auto-right">
                <span>{{eventConsumer.position}}</span>
            </td>
            <td class="cell-actions-lg">
                <button type="button" class="btn btn-text" (click)="reset()" *ngIf="eventConsumer.canReset" title="Reset Event Consumer">
                    <i class="icon icon-reset"></i>
                </button>
                <button type="button" class="btn btn-text" (click)="start()" *ngIf="eventConsumer.canStart" title="Start Event Consumer">
                    <i class="icon icon-play"></i>
                </button>
                <button type="button" class="btn btn-text" (click)="stop()" *ngIf="eventConsumer.canStop" title="Stop Event Consumer">
                    <i class="icon icon-pause"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventConsumerComponent {
    @Output()
    public error = new EventEmitter();

    @Input('sqxEventConsumer')
    public eventConsumer: EventConsumerDto;

    constructor(
        public readonly eventConsumersState: EventConsumersState
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