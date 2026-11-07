/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, NotifoComponent, PagerComponent, Router2State, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';
import { ContributorAddFormComponent } from './contributor-add-form.component';
import { ContributorComponent } from './contributor.component';

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        Router2State,
    ],
    imports: [
        ContributorAddFormComponent,
        ContributorComponent,
        FormsModule,
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
    constructor(
        private readonly contributorsRoute: Router2State,
        protected readonly contributorsState: TeamContributorsState,
    ) {
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
