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
    template: `
        <div class="table-items-footer">
            <form [formGroup]="addWorkflowForm.form" (ngSubmit)="addWorkflow()">
                <div class="row no-gutters">
                    <div class="col">
                        <sqx-control-errors for="name" [submitted]="addWorkflowForm.submitted | async"></sqx-control-errors>

                        <input type="text" class="form-control" formControlName="name" maxlength="40" placeholder="Enter workflow name" autocomplete="off" />
                    </div>
                    <div class="col-auto pl-1">
                        <button type="submit" class="btn btn-success" [disabled]="addWorkflowForm.hasNoName | async">Add Workflow</button>
                    </div>
                    <div class="col-auto pl-1">
                        <button type="reset" class="btn btn-text-secondary" (click)="cancel()">Cancel</button>
                    </div>
                </div>
            </form>
        </div>`,
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