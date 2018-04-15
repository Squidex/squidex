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

    constructor(
        public readonly authState: AuthService,
        public readonly usersState: UsersState
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public search() {
        this.usersState.search(this.usersFilter.value).onErrorResumeNext().subscribe();
    }

    public load(notify = false) {
        this.usersState.loadUsers(notify).onErrorResumeNext().subscribe();
    }

    public lock(user: UserDto) {
        this.usersState.lockUser(user).onErrorResumeNext().subscribe();
    }

    public unlock(user: UserDto) {
        this.usersState.unlockUser(user).onErrorResumeNext().subscribe();
    }

    public goPrev() {
        this.usersState.goPrev().onErrorResumeNext().subscribe();
    }

    public goNext() {
        this.usersState.goNext().onErrorResumeNext().subscribe();
    }

    public trackByUser(index: number, user: UserDto) {
        return user.id;
    }
}

