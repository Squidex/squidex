/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { UserDto, UsersState } from '@app/features/administration/internal';

@Component({
    selector: '[sqxUser]',
    template: `
        <tr [routerLink]="user.id" routerLinkActive="active">
            <td class="cell-user">
                <img class="user-picture" title="{{user.displayName}}" [src]="user | sqxUserDtoPicture" />
            </td>
            <td class="cell-auto">
                <span class="user-name table-cell">{{user.displayName}}</span>
            </td>
            <td class="cell-auto">
                <span class="user-email table-cell">{{user.email}}</span>
            </td>
            <td class="cell-actions">
                <button type="button" class="btn btn-text" (click)="lock()" sqxStopClick *ngIf="user.canLock" title="Lock User">
                    <i class="icon icon-unlocked"></i>
                </button>
                <button type="button" class="btn btn-text" (click)="unlock()" sqxStopClick *ngIf="user.canUnlock" title="Unlock User">
                    <i class="icon icon-lock"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserComponent {
    @Input('sqxUser')
    public user: UserDto;

    constructor(
        private readonly usersState: UsersState
    ) {
    }

    public lock() {
        this.usersState.lock(this.user);
    }

    public unlock() {
        this.usersState.unlock(this.user);
    }
}