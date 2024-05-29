/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, JoinPipe, SimulatedRuleEventDto, TranslatePipe } from '@app/shared';
import { RuleClassPipe, SimulatedRuleEventStatusPipe } from '../../shared/pipes';
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

const ERRORS_FAILED = [
    'Failed',
];

@Component({
    standalone: true,
    selector: '[sqxSimulatedRuleEvent]',
    styleUrls: ['./simulated-rule-event.component.scss'],
    templateUrl: './simulated-rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        JoinPipe,
        RuleClassPipe,
        RuleTransitionComponent,
        SimulatedRuleEventStatusPipe,
        TranslatePipe,
    ],
})
export class SimulatedRuleEventComponent {
    @Input('sqxSimulatedRuleEvent')
    public event!: SimulatedRuleEventDto;

    @Input({ transform: booleanAttribute })
    public expanded = false;

    @Output()
    public expandedChange = new EventEmitter<any>();

    public errorsAfterEvent = ERRORS_AFTER_EVENT;
    public errorsAfterEnrichedEvent = ERRORS_AFTER_ENRICHED_EVENT;
    public errorsFailed = ERRORS_FAILED;
}
