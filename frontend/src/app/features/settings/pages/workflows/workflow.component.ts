/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, ErrorDto, FormErrorComponent, FormRowComponent, MathHelper, SchemaTagSource, TagEditorComponent, TranslatePipe, WorkflowDto, WorkflowsState, WorkflowView } from '@app/shared';
import { WorkflowDiagramComponent } from './workflow-diagram.component';
import { WorkflowStepComponent } from './workflow-step.component';

@Component({
    selector: 'sqx-workflow',
    styleUrls: ['./workflow.component.scss'],
    templateUrl: './workflow.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
        WorkflowDiagramComponent,
        WorkflowStepComponent,
    ],
})
export class WorkflowComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    /** The workflow to show and edit. */
    public readonly workflow = input.required<WorkflowDto>();

    /** The roles that can be selected for steps and transitions. */
    public readonly roles = input.required<ReadonlyArray<string>>();

    /** The source for the schemas the workflow can be assigned to. */
    public readonly schemasSource = input.required<SchemaTagSource>();

    public readonly error = signal<ErrorDto | null | undefined>(undefined);

    public readonly isEditing = signal(false);

    public readonly isEditable = computed(() => {
        return this.workflow().canUpdate;
    });

    public readonly workflowView = linkedSignal(() => new WorkflowView(this.workflow()));

    public readonly selectedTab = signal(0);

    constructor(
        private readonly workflowsState: WorkflowsState,
    ) {
    }

    public toggleEditing() {
        this.isEditing.update(x => !x);
    }

    public remove() {
        this.workflowsState.delete(this.workflowView().dto);
    }

    public save() {
        if (!this.isEditable()) {
            return;
        }

        const workflowView = this.workflowView();

        this.workflowsState.update(workflowView.dto, workflowView.toUpdate())
            .subscribe({
                next: () => {
                    this.error.set(null);
                },
                error: (error: ErrorDto) => {
                    this.error.set(error);
                },
            });
    }

    public addStep() {
        const workflowView = this.workflowView();
        const index = workflowView.steps.length;

        for (let i = index; i < index + 100; i++) {
            const name = `Step${i}`;

            if (!workflowView.getStep(name)) {
                this.workflowView.set(workflowView.setStep(name, { color: MathHelper.randomColor() }));
                return;
            }
        }
    }

    public rename(name: string) {
        this.workflowView.update(x => x.changeName(name));
    }

    public changeSchemaIds(schemaIds: string[]) {
        this.workflowView.update(x => x.changeSchemaIds(schemaIds));
    }

    public update(updated: WorkflowView) {
        this.workflowView.set(updated);
    }

    public selectTab(tab: number) {
        this.selectedTab.set(tab);
    }
}
