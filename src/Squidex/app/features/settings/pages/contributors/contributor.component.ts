/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { ContributorDto, RoleDto } from '@app/shared';

@Component({
    selector: '[sqxContributor]',
    template: `
        <tr>
            <td class="cell-user">
                <img class="user-picture" title="{{contributor.contributorName}}" [attr.src]="contributor.contributorId | sqxUserPicture" />
            </td>
            <td class="cell-auto">
                <span class="user-name table-cell" [innerHTML]="contributor.contributorName | sqxHighlight:search"></span>
            </td>
            <td class="cell-time">
                <select class="form-control"
                    [ngModel]="contributor.role"
                    (ngModelChange)="changeRole.emit($event)"
                    [disabled]="!contributor.canUpdate">
                    <option *ngFor="let role of roles" [ngValue]="role.name">{{role.name}}</option>
                </select>
            </td>
            <td class="cell-actions">
                <button type="button" class="btn btn-text-danger" [disabled]="!contributor.canRevoke" (click)="remove.emit()">
                    <i class="icon-bin2"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContributorComponent {
    @Output()
    public changeRole = new EventEmitter<string>();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public roles: RoleDto[];

    @Input()
    public search: string;

    @Input('sqxContributor')
    public contributor: ContributorDto;
}