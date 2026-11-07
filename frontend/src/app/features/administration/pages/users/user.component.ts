/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, input, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ConfirmClickDirective, StopClickDirective, TooltipDirective, TranslatePipe, UserDtoPicture } from '@app/shared';
import { UserDto, UsersState } from '../../internal';

@Component({
    selector: '[sqxUser]',
    styleUrls: ['./user.component.scss'],
    templateUrl: './user.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        RouterLink,
        RouterLinkActive,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class UserComponent {
    private readonly usersState = inject(UsersState);

    /** The user to show in the row. */
    public readonly user = input.required<UserDto>({ alias: 'sqxUser' });

    protected lock() {
        this.usersState.lock(this.user());
    }

    protected unlock() {
        this.usersState.unlock(this.user());
    }

    protected delete() {
        this.usersState.delete(this.user());
    }
}
