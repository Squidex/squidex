/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddWorkflowForm, ControlErrorsComponent, FormHintComponent, TranslatePipe, WorkflowsState } from '@app/shared';

@Component({
    selector: 'sqx-workflow-add-form',
    styleUrls: ['./workflow-add-form.component.scss'],
    templateUrl: './workflow-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class WorkflowAddFormComponent {
    private readonly workflowsState = inject(WorkflowsState);

    protected addWorkflowForm = new AddWorkflowForm();

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
