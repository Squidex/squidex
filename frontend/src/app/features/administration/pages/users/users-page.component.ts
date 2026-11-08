/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, PagerComponent, Router2State, ShortcutDirective, SidebarMenuDirective, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { UsersState } from '../../internal';
import { UserComponent } from './user.component';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        Router2State,
    ],
    imports: [
        FormField,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        PagerComponent,
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
    private readonly usersRoute = inject(Router2State);

    protected readonly usersState = inject(UsersState);

    protected readonly usersQuery = linkedSignal(() => {
        return this.usersState.query() || '';
    });

    protected readonly usersFilter = form(this.usersQuery);

    constructor() {
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
        this.usersState.search(this.usersQuery());
    }
}
