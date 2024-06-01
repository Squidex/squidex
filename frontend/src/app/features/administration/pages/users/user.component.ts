/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ConfirmClickDirective, StopClickDirective, TooltipDirective, UserDtoPicture } from '@app/shared';
import { UserDto, UsersState } from '../../internal';

@Component({
    standalone: true,
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
        UserDtoPicture,
    ],
})
export class UserComponent {
    @Input('sqxUser')
    public user!: UserDto;

    constructor(
        private readonly usersState: UsersState,
    ) {
    }

    public lock() {
        this.usersState.lock(this.user);
    }

    public unlock() {
        this.usersState.unlock(this.user);
    }

    public delete() {
        this.usersState.delete(this.user);
    }
}
