/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

import {
    RolesState,
    SchemaTagConverter,
    WorkflowDto,
    WorkflowsState
} from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html'
})
export class WorkflowsPageComponent implements OnInit, OnDestroy {
    constructor(
        public readonly rolesState: RolesState,
        public readonly schemasSource: SchemaTagConverter,
        public readonly workflowsState: WorkflowsState
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.schemasSource.load();

        this.workflowsState.load();
    }

    public ngOnDestroy() {
        this.schemasSource.destroy();
    }

    public reload() {
        this.workflowsState.load(true);
    }

    public trackByWorkflow(workflow: WorkflowDto) {
        return workflow.id;
    }
}