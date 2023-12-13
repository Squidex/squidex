/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ContributorDto, ContributorsState, LayoutComponent, ListViewComponent, NotifoComponent, PagerComponent, RolesState, Router2State, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ContributorAddFormComponent } from './contributor-add-form.component';
import { ContributorComponent } from './contributor.component';

@Component({
    standalone: true,
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    providers: [
        Router2State,
    ],
    imports: [
        AsyncPipe,
        ContributorAddFormComponent,
        ContributorComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        NgFor,
        NgIf,
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
export class ContributorsPageComponent implements OnInit {
    constructor(
        public readonly contributorsRoute: Router2State,
        public readonly contributorsState: ContributorsState,
        public readonly rolesState: RolesState,
    ) {
    }

    public ngOnInit() {
        this.rolesState.load();

        const initial =
            this.contributorsRoute.mapTo(this.contributorsState)
                .withPaging('contributors', 10)
                .withString('query')
                .getInitial();

        this.contributorsState.load(false, initial);
    }

    public reload() {
        this.contributorsState.load(true);
    }

    public search(query: string) {
        this.contributorsState.search(query);
    }

    public trackByContributor(_index: number, contributor: ContributorDto) {
        return contributor.contributorId;
    }
}
