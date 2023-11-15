/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutComponent, ListViewComponent, PagerComponent, Router2State, ShortcutDirective, SidebarMenuDirective, Subscriptions, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { UserDto, UsersState } from '../../internal';
import { UserComponent } from './user.component';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        Router2State,
    ],
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        FormsModule,
        ReactiveFormsModule,
        NgIf,
        RouterLink,
        ListViewComponent,
        SyncWidthDirective,
        NgFor,
        UserComponent,
        PagerComponent,
        SidebarMenuDirective,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class UsersPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public usersFilter = new UntypedFormControl();

    constructor(
        public readonly usersRoute: Router2State,
        public readonly usersState: UsersState,
    ) {
        this.subscriptions.add(
            this.usersState.query
                .subscribe(q => this.usersFilter.setValue(q || '')));
    }

    public ngOnInit() {
        const initial =
            this.usersRoute.mapTo(this.usersState)
                .withPaging('users', 10)
                .withString('query')
                .getInitial();

        this.usersState.load(false, initial);
        this.usersRoute.listen();
    }

    public reload() {
        this.usersState.load(true);
    }

    public search() {
        this.usersState.search(this.usersFilter.value);
    }

    public trackByUser(_index: number, user: UserDto) {
        return user.id;
    }
}
