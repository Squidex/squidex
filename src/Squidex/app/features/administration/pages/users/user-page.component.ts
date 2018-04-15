/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { UserDto } from './../../services/users.service';
import { UserForm, UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html'
})
export class UserPageComponent implements OnDestroy, OnInit {
    private selectedUserSubscription: Subscription;

    public user?: { user: UserDto, isCurrentUser: boolean };

    public userForm = new UserForm(this.formBuilder);

    constructor(
        public readonly usersState: UsersState,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
    }

    public ngOnDestroy() {
        this.selectedUserSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.selectedUserSubscription =
            this.usersState.selectedUser
                .subscribe(selectedUser => {
                    this.user = selectedUser;

                    if (selectedUser) {
                        this.userForm.load(selectedUser.user);
                    }
                });
    }

    public save() {
        const value = this.userForm.submit();

        if (value) {
            if (this.user) {
                this.usersState.update(this.user.user, value)
                    .subscribe(user => {
                        this.userForm.submitCompleted();
                    }, error => {
                        this.userForm.submitFailed(error);
                    });
            } else {
                this.usersState.create(value)
                    .subscribe(user => {
                        this.back();
                    }, error => {
                        this.userForm.submitFailed(error);
                    });
            }
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
