/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { RolesState, SchemaTagSource, Subscriptions, WorkflowDto, WorkflowsState } from '@app/shared';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html',
})
export class WorkflowsPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public roles: ReadonlyArray<string> = [];

    constructor(
        public readonly rolesState: RolesState,
        public readonly schemasSource: SchemaTagSource,
        public readonly workflowsState: WorkflowsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.rolesState.roles
                .subscribe(roles => {
                    this.roles = roles.map(x => x.name);
                }));

        this.rolesState.load();

        this.workflowsState.load();
    }

    public reload() {
        this.workflowsState.load(true);
    }

    public trackByWorkflow(_index: number, workflow: WorkflowDto) {
        return workflow.id;
    }
}
