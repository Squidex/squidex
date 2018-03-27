/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { AppContext, UserDto } from 'shared';

import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        AppContext
    ]
})
export class UsersPageComponent implements OnInit {
    public usersFilter = new FormControl();

    constructor(public readonly ctx: AppContext,
        public readonly state: UsersState
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public search() {
        this.state.search(this.usersFilter.value);
    }

    public load(showInfo = false) {
        this.state.loadUsers()
            .subscribe(() => {
                if (showInfo) {
                    this.ctx.notifyInfo('Users reloaded.');
                }
            });
    }

    public lock(user: UserDto) {
        this.state.lockUser(user.id).subscribe();
    }

    public unlock(user: UserDto) {
        this.state.unlockUser(user.id).subscribe();
    }
}

