/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
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
    private readonly workflowsState = inject(WorkflowsState);

    protected readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    /** The workflow to show and edit. */
    public readonly workflow = input.required<WorkflowDto>();

    /** The roles that can be selected for steps and transitions. */
    public readonly roles = input.required<ReadonlyArray<string>>();

    /** The source for the schemas the workflow can be assigned to. */
    public readonly schemasSource = input.required<SchemaTagSource>();

    protected readonly error = signal<ErrorDto | null | undefined>(undefined);

    protected readonly isEditing = signal(false);

    protected readonly isEditable = computed(() => {
        return this.workflow().canUpdate;
    });

    protected readonly workflowView = linkedSignal(() => new WorkflowView(this.workflow()));

    protected readonly selectedTab = signal(0);

    protected toggleEditing() {
        this.isEditing.update(x => !x);
    }

    protected remove() {
        this.workflowsState.delete(this.workflowView().dto);
    }

    protected save() {
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

    protected addStep() {
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

    protected rename(name: string) {
        this.workflowView.update(x => x.changeName(name));
    }

    protected changeSchemaIds(schemaIds: string[]) {
        this.workflowView.update(x => x.changeSchemaIds(schemaIds));
    }

    protected update(updated: WorkflowView) {
        this.workflowView.set(updated);
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
    }
}
