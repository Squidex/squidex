/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AppContext, ValidatorsEx } from 'shared';

import { UserDto } from './../../services/users.service';
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
    public userFormError = '';
    public userForm =
        this.formBuilder.group({
            email: ['',
                [
                    Validators.email,
                    Validators.required,
                    Validators.maxLength(100)
                ]],
            displayName: ['',
                [
                    Validators.required,
                    Validators.maxLength(100)
                ]],
            password: ['',
                [
                    Validators.nullValidator
                ]],
            passwordConfirm: ['',
                [
                    ValidatorsEx.match('password', 'Passwords must be the same.')
                ]]
        });

    public user: UserDto | null;

    public isCurrentUser = false;

    constructor(public readonly ctx: AppContext,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router,
        private readonly state: UsersState
    ) {
    }

    public ngOnDestroy() {
        if (this.selectedUserSubscription) {
            this.selectedUserSubscription.unsubscribe();
        }
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

            if (!this.user) {
                this.state.createUser(requestDto)
                    .subscribe(user => {
                        this.back();
                    }, error => {
                        this.resetFormState(error.displayMessage);
                    });
            } else {
                this.state.updateUser(this.user!, requestDto)
                    .subscribe(() => {
                        this.resetFormState();
                    }, error => {
                        this.resetFormState(error.displayMessage);
                    });
            }
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.ctx.route, replaceUrl: true });
    }

    private setupAndPopulateForm(user: UserDto | null) {
        this.user = user;

        this.isCurrentUser = user !== null && user.id === this.ctx.userId;

        this.userForm.controls['password'].setValidators(
            user ?
            Validators.nullValidator :
            Validators.required);

        this.resetFormState();

        this.userForm.reset();
        this.userForm.patchValue(user || {});
    }

    private resetFormState(message: string = '') {
        this.userForm.enable();
        this.userForm.controls['password'].reset();
        this.userForm.controls['passwordConfirm'].reset();
        this.userFormSubmitted = false;
        this.userFormError = message;
    }
}

