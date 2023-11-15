/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ConfirmClickDirective, StopClickDirective, TooltipDirective, UserDtoPicture } from '@app/shared';
import { UserDto, UsersState } from '../../internal';

@Component({
    selector: '[sqxUser]',
    styleUrls: ['./user.component.scss'],
    templateUrl: './user.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        RouterLinkActive,
        RouterLink,
        TooltipDirective,
        NgIf,
        StopClickDirective,
        ConfirmClickDirective,
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
