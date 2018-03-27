/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { AuthService, DialogService } from 'shared';

import { UserDto } from './../../services/users.service';
import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        UsersState
    ]
})
export class UsersPageComponent implements OnInit {
    public usersFilter = new FormControl();

    constructor(
        public readonly usersState: UsersState,
        public readonly authState: AuthService,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public search() {
        this.usersState.search(this.usersFilter.value).subscribe();
    }

    public load(showInfo = false) {
        this.usersState.loadUsers()
            .subscribe(() => {
                if (showInfo) {
                    this.dialogs.notifyInfo('Users reloaded.');
                }
            });
    }

    public lock(user: UserDto) {
        this.usersState.lockUser(user).subscribe();
    }

    public unlock(user: UserDto) {
        this.usersState.unlockUser(user).subscribe();
    }

    public goPrev() {
        this.usersState.goPrev().subscribe();
    }

    public goNext() {
        this.usersState.goNext().subscribe();
    }
}

