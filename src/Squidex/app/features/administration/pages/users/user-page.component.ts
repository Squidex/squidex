/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import {
    AuthService,
    ComponentBase,
    MessageBus,
    NotificationService,
    UserDto,
    UserManagementService,
    ValidatorsEx
} from 'shared';

import { UserCreated, UserUpdated } from './../messages';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html'
})
export class UserPageComponent extends ComponentBase implements OnInit {
    private user: UserDto;

    public currentUserId: string;
    public userFormSubmitted = false;
    public userForm: FormGroup;
    public userId: string;
    public userFormError? = '';

    public isCurrentUser = false;
    public isNewMode = false;

    constructor(notifications: NotificationService,
        private readonly authService: AuthService,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly userManagementService: UserManagementService
    ) {
        super(notifications);
    }

    public ngOnInit() {
        this.currentUserId = this.authService.user!.id;

        this.route.data.map(p => p['user'])
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
                                created.pictureUrl!,
                                false);

                        this.emitUserCreated(this.user);
                        this.notifyInfo('User created successfully.');
                        this.back();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            } else {
                this.userManagementService.putUser(this.userId, requestDto)
                    .subscribe(() => {
                        this.user =
                            this.user.update(
                                requestDto.email,
                                requestDto.displayMessage);

                        this.emitUserUpdated(this.user);
                        this.notifyInfo('User saved successfully.');
                        this.resetUserForm();
                    }, error => {
                        this.resetUserForm(error.displayMessage);
                    });
            }
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }

    private emitUserCreated(user: UserDto) {
        this.messageBus.emit(new UserCreated(user));
    }

    private emitUserUpdated(user: UserDto) {
        this.messageBus.emit(new UserUpdated(user));
    }

    private setupAndPopulateForm() {
        const input = this.user || {};

        this.isNewMode = !this.user;
        this.userId = input['id'];
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

        this.isCurrentUser = this.userId === this.currentUserId;

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

