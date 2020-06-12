/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { UserDto, UsersState } from '@app/features/administration/internal';
import { Router2State } from '@app/framework';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        Router2State
    ]
})
export class UsersPageComponent implements OnInit {
    public usersFilter = new FormControl();

    constructor(
        public readonly usersSync: Router2State,
        public readonly usersState: UsersState
    ) {
        this.usersSync.map(usersState)
            .withPager('usersPager', 'users', 10)
            .withString('usersQuery', 'q');
    }

    public ngOnInit() {
        this.usersState.load();
    }

    public reload() {
        this.usersState.load(true);
    }

    public search() {
        this.usersState.search(this.usersFilter.value);
    }

    public trackByUser(index: number, user: UserDto) {
        return user.id;
    }
}