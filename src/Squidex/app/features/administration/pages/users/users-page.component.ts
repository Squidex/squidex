/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { UserDto } from './../../services/users.service';
import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html'
})
export class UsersPageComponent implements OnInit {
    public usersFilter = new FormControl();

    constructor(
        public readonly usersState: UsersState
    ) {
    }

    public ngOnInit() {
        this.usersState.load().onErrorResumeNext().subscribe();
    }

    public reload() {
        this.usersState.load(true).onErrorResumeNext().subscribe();
    }

    public search() {
        this.usersState.search(this.usersFilter.value).onErrorResumeNext().subscribe();
    }

    public goPrev() {
        this.usersState.goPrev().onErrorResumeNext().subscribe();
    }

    public goNext() {
        this.usersState.goNext().onErrorResumeNext().subscribe();
    }

    public lock(user: UserDto) {
        this.usersState.lock(user).onErrorResumeNext().subscribe();
    }

    public unlock(user: UserDto) {
        this.usersState.unlock(user).onErrorResumeNext().subscribe();
    }

    public trackByUser(index: number, userInfo: { user: UserDto }) {
        return userInfo.user.id;
    }
}

