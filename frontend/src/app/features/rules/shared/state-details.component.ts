/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, input } from '@angular/core';
import { BranchItem, CodeEditorComponent, FlowExecutionStateDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { HistoryStepComponent } from './history-step.component';
import { StateStepComponent } from './state-step.component';

@Component({
    selector: 'sqx-state-details',
    styleUrls: ['./state-details.component.scss'],
    templateUrl: './state-details.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        HistoryStepComponent,
        StateStepComponent,
        TranslatePipe,
    ],
})
export class StateDetailsComponent {
    /** The available steps by type, to resolve the metadata of each step. */
    public readonly availableSteps = input.required<Record<string, RuleElementDto>>();

    /** The flattened steps of the flow to show the state for. */
    public readonly branchItems = input.required<BranchItem[]>();

    /** The execution state of the flow. */
    public readonly state = input.required<FlowExecutionStateDto>();

    /** Shows the event that triggered the flow. */
    public readonly showEvent = input(true, { transform: booleanAttribute });
}
