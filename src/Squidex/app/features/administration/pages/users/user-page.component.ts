/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import {
    AuthService,
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
    public currentUserId: string;
    public userFormSubmitted = false;
    public userForm: FormGroup;
    public userId: string;
    public userFormError: string;

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
                this.populateForm(user);
            });
    }

    public save() {
        this.userFormSubmitted = true;

        if (this.userForm.valid) {
            this.userForm.disable();

            const requestDto = this.userForm.value;

            const enable = (message?: string) => {
                this.userForm.enable();
                this.userForm.controls['password'].reset();
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
                        enable();
                    }, error => {
                        enable(error.displayMessage);
                    });
            }
        }
    }

    private populateForm(user: UserDto) {
        const input = user || {};

        this.isNewMode = !user;
        this.userId = input['id'];
        this.userFormError = '';
        this.userFormSubmitted = false;
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
                    ]]
            });

        if (user) {
            this.userForm.addControl('password', new FormControl(''));
        } else {
            this.userForm.addControl('password', new FormControl(Validators.required));
        }

        this.isCurrentUser = this.userId === this.currentUserId;
    }
}

