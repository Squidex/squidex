/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, PagerComponent, Router2State, ShortcutDirective, SidebarMenuDirective, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { UsersState } from '../../internal';
import { UserComponent } from './user.component';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        Router2State,
    ],
    imports: [
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        PagerComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        SyncWidthDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
        UserComponent,
    ],
})
export class UsersPageComponent {
    protected readonly usersFilter = new UntypedFormControl();

    constructor(
        private readonly usersRoute: Router2State,
        protected readonly usersState: UsersState,
    ) {
        effect(() => {
            const query = this.usersState.query();

            this.usersFilter.setValue(query || '');
        });

        const initial =
            this.usersRoute.mapTo(this.usersState)
                .withPaging('users', 10)
                .withString('query')
                .getInitial();

        this.usersState.load(false, initial);
        this.usersRoute.listen();
    }

    protected reload() {
        this.usersState.load(true);
    }

    protected search() {
        this.usersState.search(this.usersFilter.value);
    }
}
