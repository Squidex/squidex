/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, RolesState, SchemaTagSource, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, WorkflowsState } from '@app/shared';
import { WorkflowAddFormComponent } from './workflow-add-form.component';
import { WorkflowComponent } from './workflow.component';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
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
export class WorkflowsPageComponent {
    protected readonly roles = computed(() => {
        return this.rolesState.roles().map(x => x.name);
    });

    constructor(
        private readonly rolesState: RolesState,
        protected readonly schemasSource: SchemaTagSource,
        protected readonly workflowsState: WorkflowsState,
    ) {
        this.rolesState.load();

        this.workflowsState.load();
    }

    protected reload() {
        this.workflowsState.load(true);
    }
}
