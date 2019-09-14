/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

import {
    AppsState,
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
export class WorkflowsPageComponent implements OnInit, OnDestroy {
    public schemasSource: SchemaTagConverter;

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesState: RolesState,
        public readonly schemasState: SchemasState,
        public readonly workflowsState: WorkflowsState
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.schemasSource = new SchemaTagConverter(this.schemasState);
        this.schemasState.load();

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