/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ClientDto, ClientsState, LayoutComponent, ListViewComponent, RolesState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ClientAddFormComponent } from './client-add-form.component';
import { ClientComponent } from './client.component';

@Component({
    standalone: true,
    selector: 'sqx-clients-page',
    styleUrls: ['./clients-page.component.scss'],
    templateUrl: './clients-page.component.html',
    imports: [
        AsyncPipe,
        ClientAddFormComponent,
        ClientComponent,
        LayoutComponent,
        ListViewComponent,
        NgFor,
        NgIf,
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
export class ClientsPageComponent implements OnInit {
    constructor(
        public readonly clientsState: ClientsState,
        public readonly rolesState: RolesState,
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        this.clientsState.load();
    }

    public reload() {
        this.clientsState.load(true);
    }

    public trackByClient(_index: number, client: ClientDto) {
        return client.id;
    }
}
