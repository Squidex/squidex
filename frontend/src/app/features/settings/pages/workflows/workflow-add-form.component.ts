/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AddWorkflowForm, ControlErrorsComponent, FormHintComponent, TranslatePipe, WorkflowsState } from '@app/shared';

@Component({
    selector: 'sqx-workflow-add-form',
    styleUrls: ['./workflow-add-form.component.scss'],
    templateUrl: './workflow-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ControlErrorsComponent,
        FormField,
        FormHintComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class WorkflowAddFormComponent {
    private readonly workflowsState = inject(WorkflowsState);

    protected readonly addWorkflowForm = new AddWorkflowForm();

    protected addWorkflow() {
        const value = this.addWorkflowForm.submit();
        if (!value) {
            return;
        }

        this.workflowsState.add(value)
            .subscribe({
                next: () => {
                    this.addWorkflowForm.submitCompleted();
                },
                error: error => {
                    this.addWorkflowForm.submitFailed(error);
                },
            });
    }

    protected cancel() {
        this.addWorkflowForm.submitCompleted();
    }
}
