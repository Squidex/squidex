/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TeamContributorsState } from '@app/features/teams/internal';
import { ContributorDto } from '@app/shared';

@Component({
    selector: '[sqxContributor]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContributorComponent {
    @Input()
    public search?: string | RegExp | null;

    @Input('sqxContributor')
    public contributor!: ContributorDto;

    constructor(
        private readonly contributorsState: TeamContributorsState,
    ) {
    }

    public remove() {
        this.contributorsState.revoke(this.contributor);
    }

    public changeRole(role: string) {
        this.contributorsState.assign({ contributorId: this.contributor.contributorId, role });
    }
}
