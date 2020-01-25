/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { AddWorkflowForm, WorkflowsState } from '@app/shared';

@Component({
    selector: 'sqx-workflow-add-form',
    styleUrls: ['./workflow-add-form.component.scss'],
    templateUrl: './workflow-add-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkflowAddFormComponent {
    public addWorkflowForm = new AddWorkflowForm(this.formBuilder);

    constructor(
        private readonly workflowsState: WorkflowsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public addWorkflow() {
        const value = this.addWorkflowForm.submit();

        if (value) {
            this.workflowsState.add(value.name)
                .subscribe(() => {
                    this.addWorkflowForm.submitCompleted();
                }, error => {
                    this.addWorkflowForm.submitFailed(error);
                });
        }
    }

    public cancel() {
        this.addWorkflowForm.submitCompleted();
    }
}