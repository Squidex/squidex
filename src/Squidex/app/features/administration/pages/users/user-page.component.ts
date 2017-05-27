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
    ComponentBase,
    MessageBus,
    NotificationService,
    UserDto,
    UserManagementService
} from 'shared';

import { UserCreated, UserUpdated } from './messages';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html'
})
export class UserPageComponent extends ComponentBase implements OnInit {
    public userFormSubmitted = false;
    public userForm: FormGroup;
    public userId: string;
    public userFormError: string;

    public isNewMode = false;

    constructor(notifications: NotificationService,
        private readonly formBuilder: FormBuilder,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly userManagementService: UserManagementService
    ) {
        super(notifications);
    }

    public ngOnInit() {
        this.route.data.map(p => p['user'])
            .subscribe((user: UserDto) => {
                this.populateForm(user);
            });
    }

    public save(publish: boolean) {
        this.userFormSubmitted = true;

        if (this.userForm.valid) {
            this.userForm.disable();

            const requestDto = this.userForm.value;

            const enable = (message: string) => {
                this.userForm.enable();
                this.userForm.controls['password'].reset();
                this.userForm.controls['passwordConfirm'].reset();
                this.userFormSubmitted = false;
                this.userFormError = message;
            };

            const back = () => {
                this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
            };

            if (this.isNewMode) {
                 this.userManagementService.postUser(requestDto)
                    .subscribe(created => {
                        this.messageBus.publish(
                            new UserCreated(
                                created.id,
                                requestDto.email,
                                requestDto.displayName,
                                created.pictureUrl));

                        this.notifyInfo('User created successfully.');
                        back();
                    }, error => {
                        this.notifyError(error);
                        enable(error.displayMessage);
                    });
            } else {
                 this.userManagementService.putUser(this.userId, requestDto)
                    .subscribe(() => {
                        this.messageBus.publish(
                            new UserUpdated(
                                this.userId,
                                requestDto.email,
                                requestDto.displayName));

                        this.notifyInfo('User saved successfully.');
                        enable(null);
                    }, error => {
                        this.notifyError(error);
                        enable(error.displayMessage);
                    });
            }
        } else {
            this.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private populateForm(user: UserDto) {
        this.userFormError = '';
        this.userFormSubmitted = false;

        if (user) {
            this.isNewMode = false;
            this.userId = user.id;
            this.userForm =
                this.formBuilder.group({
                    email: [user.email,
                        [
                            Validators.email,
                            Validators.required,
                            Validators.maxLength(100)
                        ]],
                    displayName: [user.displayName,
                        [
                            Validators.required,
                            Validators.maxLength(100)
                        ]],
                    password: ['', []],
                    passwordConfirm: ['', []]
                });
        } else {
            this.isNewMode = true;
            this.userForm =
                this.formBuilder.group({
                    displayName: ['',
                        [
                            Validators.required,
                            Validators.maxLength(100)
                        ]],
                    email: ['',
                        [
                            Validators.email,
                            Validators.required,
                            Validators.maxLength(100)
                        ]],
                    password: ['', [
                            Validators.required
                        ]],
                    passwordConfirm: ['', [
                            Validators.required
                        ]]
                });
        }
    }
}

