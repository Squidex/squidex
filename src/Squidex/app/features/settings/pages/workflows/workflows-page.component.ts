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
    ResourceOwner,
    RolesState,
    SchemasState,
    WorkflowDto,
    WorkflowsState
} from '@app/shared';

import { SchemaTagConverter } from './schema-tag-converter';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent extends ResourceOwner implements OnInit {
    public addWorkflowForm = new AddWorkflowForm(this.formBuilder);

    public schemasSource: SchemaTagConverter;

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesState: RolesState,
        public readonly schemasState: SchemasState,
        public readonly workflowsState: WorkflowsState,
        private readonly formBuilder: FormBuilder
    ) {
        super();
    }

    public ngOnInit() {
        this.own(this.schemasState.changes.subscribe(s => {
            if (s.isLoaded) {
                this.schemasSource = new SchemaTagConverter(s.schemas.values);
            }
        }));

        this.rolesState.load();
        this.schemasState.load();
        this.workflowsState.load();
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

