/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ControlErrorsComponent, FormErrorComponent, LayoutComponent, ShortcutDirective, Subscriptions, TitleComponent, TooltipDirective, TranslatePipe, UpdateUserDto } from '@app/shared';
import { UserDto, UserForm, UsersState } from '../../internal';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FormErrorComponent,
        FormsModule,
        LayoutComponent,
        ReactiveFormsModule,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class UserPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public isEditable = false;

    public user?: UserDto | null;
    public userForm = new UserForm();

    constructor(
        public readonly usersState: UsersState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.usersState.selectedUser
                .subscribe(user => {
                    this.user = user;

                    this.isEditable = !user || user.canUpdate;

                    const permissions: string[] = [];

                    this.userForm.load(user || { permissions });
                    this.userForm.setEnabled(this.isEditable);
                }));
    }

    public save() {
        if (!this.isEditable) {
            return;
        }

        const value = this.userForm.submit();
        if (!value) {
            return;
        }

        if (this.user) {
            const request = new UpdateUserDto({ ...value });

            this.usersState.update(this.user, request)
                .subscribe({
                    next: user => {
                        this.userForm.submitCompleted({ newValue: user });
                    },
                    error: error => {
                        this.userForm.submitFailed(error);
                    },
                });
        } else {
            this.usersState.create(value)
                .subscribe({
                    next: () => {
                        this.back();
                    },
                    error: error => {
                        this.userForm.submitFailed(error);
                    },
                });
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
