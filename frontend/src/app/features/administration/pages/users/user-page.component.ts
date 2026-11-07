/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FormErrorComponent, FormRowComponent, LayoutComponent, ShortcutDirective, TitleComponent, TooltipDirective, TranslatePipe, UpdateUserDto } from '@app/shared';
import { UserDto, UserForm, UsersState } from '../../internal';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html',
    imports: [
        AsyncPipe,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ReactiveFormsModule,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class UserPageComponent {
    private user?: UserDto | null;

    public readonly isEditable = signal(false);

    public readonly userForm = new UserForm();

    constructor(
        public readonly usersState: UsersState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
        effect(onCleanup => {
            const subscription =
                this.usersState.selectedUser
                    .subscribe(user => {
                        this.user = user;

                        const isEditable = !user || user.canUpdate;

                        this.isEditable.set(isEditable);

                        const permissions: string[] = [];

                        this.userForm.load(user || { permissions });
                        this.userForm.setEnabled(isEditable);
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public save() {
        if (!this.isEditable()) {
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
