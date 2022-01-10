/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { UserDto, UsersState } from '@app/features/administration/internal';

@Component({
    selector: '[sqxUser]',
    styleUrls: ['./user.component.scss'],
    templateUrl: './user.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
