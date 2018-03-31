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
    private user?: UserDto;

    public userForm: UserForm;

    constructor(formBuilder: FormBuilder,
        public readonly usersState: UsersState,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        this.userForm = new UserForm(formBuilder);
    }

    public ngOnDestroy() {
        this.selectedUserSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.selectedUserSubscription =
            this.usersState.selectedUser
                .subscribe(user => {
                    this.user = user;
                    this.userForm.load(user);
                });
    }

    public save() {
        const request = this.userForm.submit();

        if (request) {
            if (this.user) {
                this.usersState.updateUser(this.user, request)
                    .subscribe(user => {
                        this.userForm.submitCompleted();
                    }, error => {
                        this.userForm.submitFailed(error);
                    });
            } else {
                this.usersState.createUser(request)
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
