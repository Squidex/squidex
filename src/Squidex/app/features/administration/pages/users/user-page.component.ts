/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppContext,
    UserDto,
    ValidatorsEx
} from 'shared';

import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html',
    providers: [
        AppContext
    ]
})
export class UserPageComponent implements OnDestroy, OnInit {
    private selectedUserSubscription: Subscription;

    public userFormSubmitted = false;
    public userForm: FormGroup;
    public userFormError = '';

    public isCurrentUser = false;
    public isNewMode = false;

    constructor(public readonly ctx: AppContext,
        private readonly formBuilder: FormBuilder,
        private readonly state: UsersState
    ) {
    }

    public ngOnDestroy() {
        this.selectedUserSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.selectedUserSubscription =
            this.state.selectedUser.subscribe(user => this.setupAndPopulateForm(user!));
    }

    public save() {
        this.userFormSubmitted = true;

        if (this.userForm.valid) {
            this.userForm.disable();

            const requestDto = this.userForm.value;

            if (this.isNewMode) {
                this.state.createUser(requestDto)
                    .subscribe(() => {
                        this.ctx.notifyInfo('User created successfully.');

                        this.resetUserForm();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            } else {
                this.state.updateUser(requestDto)
                    .subscribe(() => {
                        this.ctx.notifyInfo('User saved successfully.');

                        this.resetUserForm();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            }
        }
    }

    private setupAndPopulateForm(user: UserDto | null) {
        const userData: any = user || {};

        this.isNewMode = !user;
        this.isCurrentUser = user !== null && user.id === this.ctx.userId;

        this.userForm =
            this.formBuilder.group({
                email: [userData.email,
                    [
                        Validators.email,
                        Validators.required,
                        Validators.maxLength(100)
                    ]],
                displayName: [userData.displayName,
                    [
                        Validators.required,
                        Validators.maxLength(100)
                    ]],
                password: ['',
                    [
                        this.isNewMode ? Validators.required : Validators.nullValidator
                    ]],
                passwordConfirm: ['',
                    [
                        ValidatorsEx.match('password', 'Passwords must be the same.')
                    ]]
            });


        this.resetUserForm();
    }

    private resetUserForm(message: string = '') {
        this.userForm.enable();
        this.userForm.controls['password'].reset();
        this.userForm.controls['passwordConfirm'].reset();
        this.userFormSubmitted = false;
        this.userFormError = message;
    }
}

