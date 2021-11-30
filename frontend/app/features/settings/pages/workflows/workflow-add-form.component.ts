/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AddWorkflowForm, WorkflowsState } from '@app/shared';

@Component({
    selector: 'sqx-workflow-add-form',
    styleUrls: ['./workflow-add-form.component.scss'],
    templateUrl: './workflow-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
