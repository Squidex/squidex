/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddWorkflowForm,
    AppsState,
    RolesState,
    WorkflowDto,
    WorkflowsState
} from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent implements OnInit {
    public addWorkflowForm = new AddWorkflowForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesState: RolesState,
        public readonly workflowsState: WorkflowsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.workflowsState.load();

        this.rolesState.load();
    }

    public reload() {
        this.workflowsState.load(true);
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

    public cancelAddWorkflow() {
        this.addWorkflowForm.submitCompleted();
    }

    public trackByWorkflow(index: number, workflow: WorkflowDto) {
        return workflow.id;
    }
}

