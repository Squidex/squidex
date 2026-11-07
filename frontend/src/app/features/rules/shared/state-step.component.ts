/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input, linkedSignal } from '@angular/core';
import { BranchItem, FlowExecutionStepStateDto, FlowStepDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { HistoryStepComponent } from './history-step.component';
import { RuleClassPipe } from './pipes';
import { StateAttemptComponent } from './state-attempt.component';
import { StateStepPropertyComponent } from './state-step-property.component';

@Component({
    selector: 'sqx-state-step',
    styleUrls: ['./state-step.component.scss'],
    templateUrl: './state-step.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        HistoryStepComponent,
        RuleClassPipe,
        StateAttemptComponent,
        StateStepPropertyComponent,
        TranslatePipe,
    ],
})
export class StateStepComponent {
    /** Marks the step as the first step of the flow. */
    public readonly isFirst = input.required<boolean>();

    /** Marks the step as the last step of the flow. */
    public readonly isLast = input.required<boolean>();

    /** Highlights the step as the next step to be executed. */
    public readonly isNext = input.required<boolean>();

    /** The step in the flow. */
    public readonly branchItem = input.required<BranchItem>();

    /** The metadata of the step type. */
    public readonly stepInfo = input.required<RuleElementDto>();

    /** The configuration of the step, to show the property values. */
    public readonly stepDefinition = input.required<FlowStepDto>();

    /** The execution state of the step, if it has already been executed. */
    public readonly state = input.required<FlowExecutionStepStateDto | undefined>();

    protected readonly attemptIndex = linkedSignal<number>(() => {
        return this.stepInfo().properties.length > 0 ? -1 : 0;
    });

    protected readonly isExpandable = computed(() => {
        return (this.state()?.attempts.length ?? 0) > 0 || this.stepInfo().properties.length > 0;
    });

    protected selectAttempt(attempt: number) {
        this.attemptIndex.set(attempt);
    }
}
