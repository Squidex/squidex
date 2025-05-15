/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { BranchItem, FlowExecutionStepStateDto, FlowStepDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { HistoryStepComponent } from './history-step.component';
import { RuleClassPipe } from './pipes';
import { StateAttemptComponent } from './state-attempt.component';
import { StateStepPropertyComponent } from './state-step-property.component';

@Component({
    standalone: true,
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
export class StateStepComponent implements OnChanges {
    @Input({ required: true })
    public isFirst = false;

    @Input({ required: true })
    public isLast = false;

    @Input({ required: true })
    public isNext = false;

    @Input({ required: true })
    public branchItem!: BranchItem;

    @Input({ required: true })
    public stepInfo!: RuleElementDto;

    @Input({ required: true })
    public stepDefinition!: FlowStepDto;

    @Input({ required: true })
    public state?: FlowExecutionStepStateDto;

    public attemptIndex = 0;

    public ngOnChanges() {
        this.attemptIndex = this.stepInfo.properties.length > 0 ? -1 : 0;
    }

    public selectAttempt(attempt: number) {
        this.attemptIndex = attempt;
    }
}