/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, RolesState, SchemaTagSource, ShortcutDirective, SidebarMenuDirective, Subscriptions, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, WorkflowsState } from '@app/shared';
import { WorkflowAddFormComponent } from './workflow-add-form.component';
import { WorkflowComponent } from './workflow.component';

@Component({
    standalone: true,
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html',
    imports: [
        AsyncPipe,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
        WorkflowAddFormComponent,
        WorkflowComponent,
    ],
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
}
