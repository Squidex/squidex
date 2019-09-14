/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { UserDto } from '@app/features/administration/internal';

@Component({
    selector: '[sqxUser]',
    template: `
        <tr [routerLink]="user.id" routerLinkActive="active">
            <td class="cell-user">
                <img class="user-picture" title="{{user.name}}" [attr.src]="user | sqxUserDtoPicture" />
            </td>
            <td class="cell-auto">
                <span class="user-name table-cell">{{user.displayName}}</span>
            </td>
            <td class="cell-auto">
                <span class="user-email table-cell">{{user.email}}</span>
            </td>
            <td class="cell-actions">
                <button type="button" class="btn btn-text" (click)="lock.emit()" sqxStopClick *ngIf="user.canLock" title="Lock User">
                    <i class="icon icon-unlocked"></i>
                </button>
                <button type="button" class="btn btn-text" (click)="unlock.emit()" sqxStopClick *ngIf="user.canUnlock" title="Unlock User">
                    <i class="icon icon-lock"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserComponent {
    @Output()
    public lock = new EventEmitter();

    @Output()
    public unlock = new EventEmitter();

    @Input('sqxUser')
    public user: UserDto;
}