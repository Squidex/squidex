/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { WorkflowDto, WorkflowStep, WorkflowStepValues, WorkflowTransition, WorkflowTransitionValues, WorkflowTransitionView } from '@app/shared';

@Component({
    selector: 'sqx-workflow-step[roles][step][workflow]',
    styleUrls: ['./workflow-step.component.scss'],
    templateUrl: './workflow-step.component.html',
})
export class WorkflowStepComponent implements OnChanges {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Output()
    public makeInitial = new EventEmitter();

    @Output()
    public transitionAdd = new EventEmitter<WorkflowStep>();

    @Output()
    public transitionRemove = new EventEmitter<WorkflowTransition>();

    @Output()
    public transitionUpdate = new EventEmitter<{ transition: WorkflowTransition; values: WorkflowTransitionValues }>();

    @Output()
    public update = new EventEmitter<WorkflowStepValues>();

    @Output()
    public rename = new EventEmitter<string>();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public workflow!: WorkflowDto;

    @Input()
    public step!: WorkflowStep;

    @Input()
    public roles!: ReadonlyArray<string>;

    @Input()
    public disabled?: boolean | null;

    public openSteps!: ReadonlyArray<WorkflowStep>;
    public openStep!: WorkflowStep;

    public transitions!: ReadonlyArray<WorkflowTransitionView>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['workflow'] || changes['step']) {
            this.openSteps = this.workflow.getOpenSteps(this.step);
            this.openStep = this.openSteps[0];

            this.transitions = this.workflow.getTransitions(this.step);
        }
    }

    public changeTransition(transition: WorkflowTransition, values: WorkflowTransitionValues) {
        this.transitionUpdate.emit({ transition, values });
    }

    public changeName(name: string) {
        this.rename.emit(name);
    }

    public changeColor(color: string) {
        this.update.emit({ color });
    }

    public changeValidate(validate: boolean) {
        this.update.emit({ validate });
    }

    public changeNoUpdate(noUpdate: boolean) {
        this.update.emit({ noUpdate });
    }

    public changeNoUpdateExpression(noUpdateExpression?: string) {
        this.update.emit({ noUpdateExpression });
    }

    public changeNoUpdateRoles(noUpdateRoles?: ReadonlyArray<string>) {
        this.update.emit({ noUpdateRoles });
    }

    public trackByTransition(_index: number, transition: WorkflowTransitionView) {
        return transition.to;
    }
}
