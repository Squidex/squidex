/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BranchItem, ConfirmClickDirective, FlowView, FromNowPipe, RuleElementDto, RuleEventDto, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { RuleClassPipe } from '../../shared/pipes';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { StateDetailsComponent } from '../../shared/state-details.component';

@Component({
    selector: '[sqxRuleEvent]',
    styleUrls: ['./rule-event.component.scss'],
    templateUrl: './rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FormsModule,
        FromNowPipe,
        RuleClassPipe,
        RuleElementComponent,
        StateDetailsComponent,
        TranslatePipe,
    ]
})
export class RuleEventComponent {
    @Input('sqxRuleEvent')
    public event!: RuleEventDto;

    @Input({ required: true })
    public availableSteps: Record<string, RuleElementDto> = {};

    @Input({ transform: booleanAttribute })
    public expanded = false;

    @Output()
    public expandedChange = new EventEmitter<any>();

    @Output()
    public enqueue = new EventEmitter<any>();

    @Output()
    public cancel = new EventEmitter<any>();

    public selectedStep: string | null = null;

    public branchItems: BranchItem[] = [];

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.event) {
            this.branchItems = new FlowView(this.event.flowState.definition as any).getAllItems();
        }
    }

    public selectStep(id: string) {
        if (this.selectedStep === id) {
            this.selectedStep = null;
        } else {
            this.selectedStep = id;
        }
    }
}
