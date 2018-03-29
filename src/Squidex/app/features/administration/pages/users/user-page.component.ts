/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import {
    AppContext,
    UserDto,
    UserManagementService,
    ValidatorsEx
} from 'shared';

import { UserCreated, UserUpdated } from './../messages';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html',
    providers: [
        AppContext
    ]
})
export class UserPageComponent  implements OnInit {
    public user: UserDto;

    public userFormSubmitted = false;
    public userForm: FormGroup;
    public userFormError = '';

    public isCurrentUser = false;
    public isNewMode = false;

    constructor(public readonly ctx: AppContext,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router,
        private readonly userManagementService: UserManagementService
    ) {
    }

    public ngOnInit() {
        this.ctx.route.data.map(d => d.user)
            .subscribe((user: UserDto) => {
                this.user = user;

                this.setupAndPopulateForm();
            });
    }

    public save() {
        this.userFormSubmitted = true;

        if (this.userForm.valid) {
            this.userForm.disable();

            const requestDto = this.userForm.value;

            if (this.isNewMode) {
                this.userManagementService.postUser(requestDto)
                    .subscribe(created => {
                        this.user =
                            new UserDto(
                                created.id,
                                requestDto.email,
                                requestDto.displayName,
                                false);

                        this.ctx.notifyInfo('User created successfully.');

                        this.emitUserCreated(this.user);
                        this.back();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            } else {
                this.userManagementService.putUser(this.user.id, requestDto)
                    .subscribe(() => {
                        this.user =
                            this.user.update(
                                requestDto.email,
                                requestDto.displayMessage);

                        this.ctx.notifyInfo('User saved successfully.');

                        this.emitUserUpdated(this.user);
                        this.resetUserForm();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            }
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.ctx.route, replaceUrl: true });
    }

    private emitUserCreated(user: UserDto) {
        this.ctx.bus.emit(new UserCreated(user));
    }

    private emitUserUpdated(user: UserDto) {
        this.ctx.bus.emit(new UserUpdated(user));
    }

    private setupAndPopulateForm() {
        const input = this.user || {};

        this.isNewMode = !this.user;
        this.userForm =
            this.formBuilder.group({
                email: [input['email'],
                    [
                        Validators.email,
                        Validators.required,
                        Validators.maxLength(100)
                    ]],
                displayName: [input['displayName'],
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

        this.isCurrentUser = this.user && this.user.id === this.ctx.userId;

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

