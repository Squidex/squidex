/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { BranchItem, FlowExecutionStepStateDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { HistoryStepComponent } from './history-step.component';
import { RuleClassPipe } from './pipes';
import { StateAttemptComponent } from './state-attempt.component';

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
        TranslatePipe,
    ],
})
export class StateStepComponent {
    @Input({ required: true })
    public isFirst = false;

    @Input({ required: true })
    public isLast = false;

    @Input({ required: true })
    public isNext = false;

    @Input({ required: true })
    public stepItem!: BranchItem;

    @Input({ required: true })
    public stepInfo!: RuleElementDto;

    @Input({ required: true })
    public state?: FlowExecutionStepStateDto;

    public attemptIndex = 0;

    public selectAttempt(attempt: number) {
        this.attemptIndex = attempt;
    }
}