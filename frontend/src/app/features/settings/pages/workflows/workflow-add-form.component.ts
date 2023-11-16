/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AddWorkflowForm, ControlErrorsComponent, FormHintComponent, TranslatePipe, WorkflowsState } from '@app/shared';

@Component({
    standalone: true,
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
    public addWorkflowForm = new AddWorkflowForm();

    constructor(
        private readonly workflowsState: WorkflowsState,
    ) {
    }

    public addWorkflow() {
        const value = this.addWorkflowForm.submit();

        if (value) {
            this.workflowsState.add(value.name)
                .subscribe({
                    next: () => {
                        this.addWorkflowForm.submitCompleted();
                    },
                    error: error => {
                        this.addWorkflowForm.submitFailed(error);
                    },
                });
        }
    }

    public cancel() {
        this.addWorkflowForm.submitCompleted();
    }
}
