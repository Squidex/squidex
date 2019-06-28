/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppsState,
    MathHelper,
    RolesState,
    WorkflowDto,
    WorkflowsState,
    WorkflowStep,
    WorkflowStepValues,
    WorkflowTransition,
    WorkflowTransitionValues
} from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent implements OnInit {
    private maxSteps = 1;

    public workflow: WorkflowDto;

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesState: RolesState,
        public readonly workflowsState: WorkflowsState
    ) {
    }

    public ngOnInit() {
        this.workflowsState.load()
            .subscribe(workflow => {
                this.workflow = workflow;
            });

        this.rolesState.load();
    }

    public reload() {
        this.workflowsState.load(true)
            .subscribe(workflow => {
                this.workflow = workflow;
            });
    }

    public save() {
        this.workflowsState.save(this.workflow);
    }

    public addStep() {
        this.workflow = this.workflow.setStep(`Step${this.maxSteps}`, { color: MathHelper.randomColor() });

        this.maxSteps++;
    }

    public setInitial(step: WorkflowStep) {
        this.workflow = this.workflow.setInitial(step.name);
    }

    public addTransiton(from: WorkflowStep, to: WorkflowStep) {
        this.workflow = this.workflow.setTransition(from.name, to.name, {});
    }

    public removeTransition(from: WorkflowStep, transition: WorkflowTransition) {
        this.workflow = this.workflow.removeTransition(from.name, transition.to);
    }

    public updateTransition(update: { transition: WorkflowTransition, values: WorkflowTransitionValues }) {
        this.workflow = this.workflow.setTransition(update.transition.from, update.transition.to, update.values);
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

