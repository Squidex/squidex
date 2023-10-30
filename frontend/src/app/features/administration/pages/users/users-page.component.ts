/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { UserDto, UsersState } from '@app/features/administration/internal';
import { Router2State, Subscriptions } from '@app/framework';

@Component({
    selector: 'sqx-users-page',
    styleUrls: ['./users-page.component.scss'],
    templateUrl: './users-page.component.html',
    providers: [
        Router2State,
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
