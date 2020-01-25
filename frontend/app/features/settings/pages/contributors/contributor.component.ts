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
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
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