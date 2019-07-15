/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

import { UserDto, UsersState } from '@app/features/administration/internal';

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
        this.usersState.load();
    }

    public reload() {
        this.usersState.load(true);
    }

    public search() {
        this.usersState.search(this.usersFilter.value);
    }

    public goPrev() {
        this.usersState.goPrev();
    }

    public goNext() {
        this.usersState.goNext();
    }

    public lock(user: UserDto) {
        this.usersState.lock(user);
    }

    public unlock(user: UserDto) {
        this.usersState.unlock(user);
    }

    public trackByUser(index: number, user: UserDto) {
        return user.id;
    }
}

