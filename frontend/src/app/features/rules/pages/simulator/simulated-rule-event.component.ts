/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CodeEditorComponent, FlowView, JoinPipe, RuleElementDto, SimulatedRuleEventDto, TranslatePipe } from '@app/shared';
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
        HistoryStepComponent,
        JoinPipe,
        RuleClassPipe,
        RuleTransitionComponent,
        SimulatedRuleEventStatusPipe,
        StateDetailsComponent,
        TranslatePipe,
    ],
})
export class SimulatedRuleEventComponent {
    /** The available steps by type, to resolve the metadata of each step. */
    public readonly availableSteps = input.required<Record<string, RuleElementDto>>();

    /** The simulated event to show. */
    public readonly event = input.required<SimulatedRuleEventDto>({ alias: 'sqxSimulatedRuleEvent' });

    /** Shows the details of the event. */
    public readonly expanded = input(false, { transform: booleanAttribute });

    /** Emits when the user wants to toggle the details. */
    public readonly expandedChange = output();

    protected readonly errorsAfterEvent = ERRORS_AFTER_EVENT;
    protected readonly errorsAfterEnrichedEvent = ERRORS_AFTER_ENRICHED_EVENT;

    protected readonly branchItems = computed(() => {
        const flowState = this.event().flowState;
        if (!flowState) {
            return [];
        }

        return new FlowView(flowState.definition as any).getAllItems();
    });
}
