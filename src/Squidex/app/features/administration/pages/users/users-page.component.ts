/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { AuthService } from '@app/shared';

import { UserDto } from './../../services/users.service';
import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersPageComponent implements OnInit {
    public usersFilter = new FormControl();

    public users =
        this.usersState.changes.map(x => x.users);

    public usersPager =
        this.usersState.changes.map(x => x.usersPager);

    constructor(
        public readonly authState: AuthService,
        public readonly usersState: UsersState
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public search() {
        this.usersState.search(this.usersFilter.value).subscribe();
    }

    public load(notify = false) {
        this.usersState.loadUsers(notify).subscribe();
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

