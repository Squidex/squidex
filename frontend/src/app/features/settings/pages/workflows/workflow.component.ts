/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, ErrorDto, FormErrorComponent, FormHintComponent, MathHelper, SchemaTagSource, TagEditorComponent, TranslatePipe, WorkflowDto, WorkflowsState, WorkflowView } from '@app/shared';
import { WorkflowDiagramComponent } from './workflow-diagram.component';
import { WorkflowStepComponent } from './workflow-step.component';

@Component({
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
    public set workflow(value: WorkflowDto) {
        this.workflowView = new WorkflowView(value);
    }

    @Input({ required: true })
    public roles!: ReadonlyArray<string>;

    @Input({ required: true })
    public schemasSource!: SchemaTagSource;

    public error?: ErrorDto | null;

    public isEditing = false;
    public isEditable = false;

    public workflowView!: WorkflowView;

    public selectedTab = 0;

    constructor(
        private readonly workflowsState: WorkflowsState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.workflowView.dto.canUpdate;
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public remove() {
        this.workflowsState.delete(this.workflowView.dto);
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        this.workflowsState.update(this.workflowView.dto, this.workflowView.toUpdate())
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
        const index = this.workflowView.steps.length;

        for (let i = index; i < index + 100; i++) {
            const name = `Step${i}`;

            if (!this.workflowView.getStep(name)) {
                this.workflowView = this.workflowView.setStep(name, { color: MathHelper.randomColor() });
                return;
            }
        }
    }

    public rename(name: string) {
        this.workflowView = this.workflowView.changeName(name);
    }

    public changeSchemaIds(schemaIds: string[]) {
        this.workflowView = this.workflowView.changeSchemaIds(schemaIds);
    }

    public update(updated: WorkflowView) {
        this.workflowView = updated;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }
}
