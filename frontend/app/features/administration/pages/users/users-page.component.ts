/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { UserDto, UsersState } from '@app/features/administration/internal';
import { ResourceOwner, Router2State } from '@app/framework';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        Router2State
    ]
})
export class UsersPageComponent extends ResourceOwner implements OnInit {
    public usersFilter = new FormControl();

    constructor(
        public readonly usersRoute: Router2State,
        public readonly usersState: UsersState
    ) {
        super();

        this.own(
            this.usersState.usersQuery
                .subscribe(q => this.usersFilter.setValue(q || '')));
    }

    public ngOnInit() {
        this.usersState.loadAndListen(this.usersRoute);
    }

    public reload() {
        this.usersState.load(true);
    }

    public search() {
        this.usersState.search(this.usersFilter.value);
    }

    public trackByUser(_ndex: number, user: UserDto) {
        return user.id;
    }
}