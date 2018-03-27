/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService, ValidatorsEx } from 'shared';

import { UserDto } from './../../services/users.service';
import { UsersState } from './../../state/users.state';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html'
})
export class UserPageComponent implements OnInit {
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
    public isNotFound = false;

    constructor(
        private readonly authService: AuthService,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly usersState: UsersState
    ) {
    }

    public ngOnInit() {
        this.route.params.map(p => p['userId'])
            .switchMap(id => this.usersState.selectUser(id).map(u => { return { user: u, expected: !!id }; }))
            .subscribe(result => {
                this.isNotFound = !result.user && result.expected;

                this.setupAndPopulateForm(result.user);
            });
    }

    public save() {
        this.userFormSubmitted = true;

        if (this.userForm.valid) {
            this.userForm.disable();

            const requestDto = this.userForm.value;

            if (!this.user) {
                this.usersState.createUser(requestDto)
                    .subscribe(user => {
                        this.back();
                    }, error => {
                        this.resetFormState(error.displayMessage);
                    });
            } else {
                this.usersState.updateUser(this.user!, requestDto)
                    .subscribe(() => {
                        this.resetFormState();
                    }, error => {
                        this.resetFormState(error.displayMessage);
                    });
            }
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }

    private setupAndPopulateForm(user: UserDto | null) {
        this.user = user;

        this.isCurrentUser = user !== null && user.id === this.authService.user!.id;

        this.userForm.controls['password'].setValidators(
            user ?
            Validators.nullValidator :
            Validators.required);

        this.resetFormState();

        this.userForm.reset();
        this.userForm.patchValue(user || {});
    }

    private resetFormState(message: string = '') {
        this.userFormSubmitted = false;
        this.userFormError = message;
        this.userForm.enable();
        this.userForm.controls['password'].reset();
        this.userForm.controls['passwordConfirm'].reset();
    }
}

