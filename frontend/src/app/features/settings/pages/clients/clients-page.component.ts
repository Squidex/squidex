/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ClientsState, LayoutComponent, ListViewComponent, RolesState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ClientAddFormComponent } from './client-add-form.component';
import { ClientComponent } from './client.component';

@Component({
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ClientAddFormComponent,
        ClientComponent,
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
    ],
})
export class ClientsPageComponent {
    protected readonly clientsState = inject(ClientsState);

    protected readonly rolesState = inject(RolesState);

    constructor() {
        this.rolesState.load();

        this.clientsState.load();
    }

    protected reload() {
        this.clientsState.load(true);
    }
}
