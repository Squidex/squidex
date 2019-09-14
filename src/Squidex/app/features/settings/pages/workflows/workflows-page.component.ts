/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
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
    public schemasSource: SchemaTagConverter;

    constructor(
        public readonly appsState: AppsState,
        public readonly rolesState: RolesState,
        public readonly schemasState: SchemasState,
        public readonly workflowsState: WorkflowsState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.changes
                .subscribe(state => {
                    if (state.isLoaded) {
                        this.schemasSource = new SchemaTagConverter(state.schemas.values);
                    }
                }));

        this.rolesState.load();
        this.schemasState.load();
        this.workflowsState.load();
    }

    public reload() {
        this.workflowsState.load(true);
    }

    public trackByWorkflow(workflow: WorkflowDto) {
        return workflow.id;
    }
}

