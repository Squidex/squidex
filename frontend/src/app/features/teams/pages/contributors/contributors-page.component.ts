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
import { ContributorDto, LayoutComponent, ListViewComponent, NotifoComponent, PagerComponent, Router2State, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';
import { ContributorAddFormComponent } from './contributor-add-form.component';
import { ContributorComponent } from './contributor.component';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    providers: [
        Router2State,
    ],
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        NotifoComponent,
        TooltipDirective,
        ShortcutDirective,
        FormsModule,
        ListViewComponent,
        NgIf,
        ContributorAddFormComponent,
        NgFor,
        ContributorComponent,
        PagerComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class ContributorsPageComponent implements OnInit {
    constructor(
        public readonly contributorsRoute: Router2State,
        public readonly contributorsState: TeamContributorsState,
    ) {
    }

    public ngOnInit() {
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
