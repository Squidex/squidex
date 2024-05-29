/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, ErrorDto, FormErrorComponent, FormHintComponent, MathHelper, SchemaTagSource, TagEditorComponent, TranslatePipe, WorkflowDto, WorkflowsState, WorkflowStep, WorkflowStepValues, WorkflowTransition, WorkflowTransitionValues } from '@app/shared';
import { WorkflowDiagramComponent } from './workflow-diagram.component';
import { WorkflowStepComponent } from './workflow-step.component';

@Component({
    standalone: true,
    selector: 'sqx-workflow',
    styleUrls: ['./workflow.component.scss'],
    templateUrl: './workflow.component.html',
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
        WorkflowDiagramComponent,
        WorkflowStepComponent,
    ],
})
export class WorkflowComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Input({ required: true })
    public workflow!: WorkflowDto;

    @Input({ required: true })
    public roles!: ReadonlyArray<string>;

    @Input({ required: true })
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
}
