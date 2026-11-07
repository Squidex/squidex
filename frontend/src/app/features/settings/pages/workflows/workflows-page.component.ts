/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { map } from 'rxjs/operators';
import { LayoutComponent, ListViewComponent, RolesState, SchemaTagSource, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, WorkflowsState } from '@app/shared';
import { WorkflowAddFormComponent } from './workflow-add-form.component';
import { WorkflowComponent } from './workflow.component';

@Component({
    selector: 'sqx-workflows-page',
    styleUrls: ['./workflows-page.component.scss'],
    templateUrl: './workflows-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
export class WorkflowsPageComponent {
    public readonly roles = toSignal(this.rolesState.roles.pipe(map(roles => roles.map(x => x.name))), { initialValue: [] });

    constructor(
        public readonly rolesState: RolesState,
        public readonly schemasSource: SchemaTagSource,
        public readonly workflowsState: WorkflowsState,
    ) {
        this.rolesState.load();

        this.workflowsState.load();
    }

    public reload() {
        this.workflowsState.load(true);
    }
}
