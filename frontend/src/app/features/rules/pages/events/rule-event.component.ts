/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, ConfirmClickDirective, FromNowPipe, RuleEventDto, TranslatePipe } from '@app/shared';
import { RuleClassPipe } from '../../shared/pipes';

@Component({
    standalone: true,
    selector: '[sqxRuleEvent]',
    styleUrls: ['./rule-event.component.scss'],
    templateUrl: './rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        FormsModule,
        FromNowPipe,
        RuleClassPipe,
        TranslatePipe,
    ],
})
export class RuleEventComponent {
    @Input('sqxRuleEvent')
    public event!: RuleEventDto;

    @Input({ transform: booleanAttribute })
    public expanded = false;

    @Output()
    public expandedChange = new EventEmitter<any>();

    @Output()
    public enqueue = new EventEmitter<any>();

    @Output()
    public cancel = new EventEmitter<any>();
}
