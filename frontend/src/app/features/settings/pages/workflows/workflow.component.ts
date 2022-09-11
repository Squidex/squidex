/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { ErrorDto, MathHelper, SchemaTagSource, WorkflowDto, WorkflowsState, WorkflowStep, WorkflowStepValues, WorkflowTransition, WorkflowTransitionValues } from '@app/shared';

@Component({
    selector: 'sqx-workflow[roles][schemasSource][workflow]',
    styleUrls: ['./workflow.component.scss'],
    templateUrl: './workflow.component.html',
})
export class WorkflowComponent implements OnChanges {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Input()
    public workflow!: WorkflowDto;

    @Input()
    public roles!: ReadonlyArray<string>;

    @Input()
    public schemasSource!: SchemaTagSource;

    public error?: ErrorDto | null;

    public isEditing = false;
    public isEditable = false;

    public selectedTab = 0;

    constructor(
        private readonly workflowsState: WorkflowsState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.workflow.canUpdate;
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public remove() {
        this.workflowsState.delete(this.workflow);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        this.workflowsState.update(this.workflow)
            .subscribe({
                next: () => {
                    this.error = null;
                },
                error: (error: ErrorDto) => {
                    this.error = error;
                },
            });
    }

    public addStep() {
        const index = this.workflow.steps.length;

        for (let i = index; i < index + 100; i++) {
            const name = `Step${i}`;

            if (!this.workflow.getStep(name)) {
                this.workflow = this.workflow.setStep(name, { color: MathHelper.randomColor() });
                return;
            }
        }
    }

    public rename(name: string) {
        this.workflow = this.workflow.changeName(name);
    }

    public changeSchemaIds(schemaIds: string[]) {
        this.workflow = this.workflow.changeSchemaIds(schemaIds);
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

    public updateTransition(update: { transition: WorkflowTransition; values: WorkflowTransitionValues }) {
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

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public trackByStep(_index: number, step: WorkflowStep) {
        return step.name;
    }
}
