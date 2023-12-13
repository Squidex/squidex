/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ConfirmClickDirective, ContributorDto, HighlightPipe, TooltipDirective, UserPicturePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';

@Component({
    standalone: true,
    selector: '[sqxContributor]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        HighlightPipe,
        TooltipDirective,
        UserPicturePipe,
    ],
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
