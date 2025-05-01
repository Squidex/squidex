/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { BranchItem, FlowExecutionStepStateDto, RuleElementDto, TranslatePipe } from '@app/shared';
import { RuleClassPipe } from './pipes';
import { RuleElementComponent } from './rule-element.component';
import { StateAttemptComponent } from './state-attempt.component';

@Component({
    standalone: true,
    selector: 'sqx-state-step',
    styleUrls: ['./state-step.component.scss'],
    templateUrl: './state-step.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RuleElementComponent,
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

    public isExpanded = false;

    public toggle() {
        this.isExpanded = !this.isExpanded;
    }

    public selectAttempt(attempt: number) {
        this.attemptIndex = attempt;
    }
}