/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { ResourceOwner } from '@app/shared';

import {
    CreateUserDto,
    UserDto,
    UserForm,
    UsersState
} from '@app/features/administration/internal';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html'
})
export class UserPageComponent extends ResourceOwner implements OnInit {
    public isEditable = true;

    public user?: UserDto;
    public userForm = new UserForm(this.formBuilder);

    constructor(
        public readonly usersState: UsersState,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.usersState.selectedUser
                .subscribe(selectedUser => {
                    this.user = selectedUser!;

                    if (selectedUser) {
                        this.isEditable = this.user.canUpdate;

                        this.userForm.load(selectedUser);
                        this.userForm.setEnabled(this.isEditable);
                    }
                }));
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.userForm.submit();

        if (value) {
            if (this.user) {
                this.usersState.update(this.user, value)
                    .subscribe(() => {
                        this.userForm.submitCompleted();
                    }, error => {
                        this.userForm.submitFailed(error);
                    });
            } else {
                this.usersState.create(<CreateUserDto>value)
                    .subscribe(() => {
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
