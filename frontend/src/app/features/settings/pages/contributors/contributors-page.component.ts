/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ContributorsState, LayoutComponent, ListViewComponent, NotifoComponent, PagerComponent, RolesState, Router2State, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ContributorAddFormComponent } from './contributor-add-form.component';
import { ContributorComponent } from './contributor.component';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    providers: [
        Router2State,
    ],
    imports: [
        ContributorAddFormComponent,
        ContributorComponent,
        LayoutComponent,
        ListViewComponent,
        NotifoComponent,
        PagerComponent,
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
export class ContributorsPageComponent {
    private readonly contributorsRoute = inject(Router2State);

    protected readonly contributorsState = inject(ContributorsState);

    protected readonly rolesState = inject(RolesState);

    constructor() {
        this.rolesState.load();

        const initial =
            this.contributorsRoute.mapTo(this.contributorsState)
                .withPaging('contributors', 10)
                .withString('query')
                .getInitial();

        this.contributorsState.load(false, initial);
    }

    protected reload() {
        this.contributorsState.load(true);
    }

    protected search(query: string) {
        this.contributorsState.search(query);
    }
}
