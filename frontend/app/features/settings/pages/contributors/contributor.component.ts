/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import {
    ContributorDto,
    ContributorsState,
    RoleDto
} from '@app/shared';

@Component({
    selector: '[sqxContributor]',
    template: `
        <tr>
            <td class="cell-user">
                <img class="user-picture" title="{{contributor.contributorName}}" [src]="contributor.contributorId | sqxUserPicture" />
            </td>
            <td class="cell-auto">
                <span class="user-name table-cell" [innerHTML]="contributor.contributorName | sqxHighlight:search"></span>
            </td>
            <td class="cell-time">
                <select class="form-control"
                    [ngModel]="contributor.role"
                    (ngModelChange)="changeRole($event)"
                    [disabled]="!contributor.canUpdate">
                    <option *ngFor="let role of roles" [ngValue]="role.name">{{role.name}}</option>
                </select>
            </td>
            <td class="cell-actions">
                <button type="button" class="btn btn-text-danger"
                    [disabled]="!contributor.canRevoke"
                    (sqxConfirmClick)="remove()"
                    confirmTitle="Remove contributor"
                    confirmText="Do you really want to remove the contributor?">
                    <i class="icon-bin2"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContributorComponent {
    @Input()
    public roles: ReadonlyArray<RoleDto>;

    @Input()
    public search: string;

    @Input('sqxContributor')
    public contributor: ContributorDto;

    constructor(
        private readonly contributorsState: ContributorsState
    ) {
    }

    public remove() {
        this.contributorsState.revoke(this.contributor);
    }

    public changeRole(role: string) {
        this.contributorsState.assign({ contributorId: this.contributor.contributorId, role });
    }
}