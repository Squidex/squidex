/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    WorkflowDto,
    WorkflowStep,
    WorkflowStepValues,
    WorkflowTransition
} from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent implements OnInit {
    public workflow: WorkflowDto;

    public ngOnInit() {
        this.workflow = new WorkflowDto().setStep('Published', { color: 'green', isLocked: true });
    }

    public reload() {
        return;
    }

    public save() {
        return;
    }

    public addStep() {
        this.workflow = this.workflow.setStep(`Step${this.workflow.steps.length + 1}`, {});
    }

    public addTransiton(from: WorkflowStep, to: WorkflowStep) {
        this.workflow = this.workflow.setTransition(from.name, to.name, {});
    }

    public removeTransition(from: WorkflowStep, transition: WorkflowTransition) {
        this.workflow = this.workflow.removeTransition(from.name, transition.to);
    }

    public updateStep(step: WorkflowStep, values: WorkflowStepValues) {
        this.workflow = this.workflow.setStep(step.name, values);
    }

    public renameStep(step: WorkflowStep, newName: string) {
        this.workflow = this.workflow.renameStep(step.name, newName);
    }

    public removeStep(step: WorkflowStep) {
        this.workflow = this.workflow.removeStep(step.name);
    }

    public trackByStep(index: number, step: WorkflowStep) {
        return step.name;
    }
}

