/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { ActivatedRoute, Router } from '@angular/router';
import { tap } from 'rxjs';
import { FormErrorComponent, FormRowComponent, LayoutComponent, ShortcutDirective, TitleComponent, TooltipDirective, TranslatePipe, UpdateUserDto } from '@app/shared';
import { UserDto, UserForm, UsersState } from '../../internal';

@Component({
    selector: 'sqx-user-page',
    styleUrls: ['./user-page.component.scss'],
    templateUrl: './user-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        LayoutComponent,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class UserPageComponent {
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private user?: UserDto | null;

    protected readonly usersState = inject(UsersState);

    protected readonly isEditable = signal(false);

    protected readonly userForm = new UserForm();

    constructor() {
        effect(() => {
            const user = this.usersState.selectedUser();
            this.user = user;

            const isEditable = !user || user.canUpdate;
            this.isEditable.set(isEditable);

            const permissions: string[] = [];

            this.userForm.load(user || { permissions });
            this.userForm.setEnabled(isEditable);
        });
    }

    protected save() {
        if (!this.isEditable()) {
            return;
        }

        const user = this.user;
        if (user) {
            this.userForm.submitAndUpdate(value => {
                return this.usersState.update(user, new UpdateUserDto({ ...value }));
            }, { resetAfterSubmit: true });
        } else {
            this.userForm.submit(value => {
                return this.usersState.create(value).pipe(tap(() => this.back()));
            });
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
