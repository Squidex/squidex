/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import {
    WorkflowDto,
    WorkflowStep,
    WorkflowStepValues
} from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent {
    public workflow = new WorkflowDto().setStep('Published', { color: 'green' });

    public reload() {
        return;
    }

    public save() {
        return;
    }

    public addStep() {
        this.workflow = this.workflow.setStep(`Step${this.workflow.steps.length + 1}`, {});
    }

    public updateStep(step: WorkflowStep, values: WorkflowStepValues) {
        // this.workflow = this.workflow.setStep(step.name, values);
    }

    public renameStep(step: WorkflowStep, newName: string) {
        // this.workflow = this.workflow.renameStep(step.name, newName);
    }

    public removeStep(step: WorkflowStep) {
        // this.workflow = this.workflow.removeStep(step.name);
    }
}

