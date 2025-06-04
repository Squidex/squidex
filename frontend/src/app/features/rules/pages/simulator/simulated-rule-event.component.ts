/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BranchItem, CodeEditorComponent, FlowView, JoinPipe, RuleElementDto, SimulatedRuleEventDto, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { HistoryStepComponent } from '../../shared/history-step.component';
import { RuleClassPipe, SimulatedRuleEventStatusPipe } from '../../shared/pipes';
import { StateDetailsComponent } from '../../shared/state-details.component';
import { RuleTransitionComponent } from './rule-transition.component';

const ERRORS_AFTER_EVENT = [
    'ConditionPrecheckDoesNotMatch',
    'Disabled',
    'FromRule',
    'NoAction',
    'NoTrigger',
    'TooOld',
    'WrongEvent',
    'WrongEventForTrigger',
];

const ERRORS_AFTER_ENRICHED_EVENT = [
    'ConditionDoesNotMatch',
];

@Component({
    selector: '[sqxSimulatedRuleEvent]',
    styleUrls: ['./simulated-rule-event.component.scss'],
    templateUrl: './simulated-rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        HistoryStepComponent,
        JoinPipe,
        RuleClassPipe,
        RuleTransitionComponent,
        SimulatedRuleEventStatusPipe,
        StateDetailsComponent,
        TranslatePipe,
    ]
})
export class SimulatedRuleEventComponent {
    @Input({ required: true })
    public availableSteps: Record<string, RuleElementDto> = {};

    @Input('sqxSimulatedRuleEvent')
    public event!: SimulatedRuleEventDto;

    @Input({ transform: booleanAttribute })
    public expanded = false;

    @Output()
    public expandedChange = new EventEmitter<any>();

    public errorsAfterEvent = ERRORS_AFTER_EVENT;
    public errorsAfterEnrichedEvent = ERRORS_AFTER_ENRICHED_EVENT;

    public branchItems: BranchItem[] = [];

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.event) {
            if (this.event.flowState) {
                this.branchItems = new FlowView(this.event.flowState.definition as any).getAllItems();
            } else {
                this.branchItems = [];
            }
        }
    }
}
