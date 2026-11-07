/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AssignContributorDto, ConfirmClickDirective, ContributorDto, HighlightPipe, TooltipDirective, TranslatePipe, UserPicturePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';

@Component({
    selector: '[sqxContributor]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        HighlightPipe,
        TooltipDirective,
        TranslatePipe,
        UserPicturePipe,
    ],
})
export class ContributorComponent {
    /** The search term to highlight in the name of the contributor. */
    public readonly search = input<string | RegExp | null>();

    /** The contributor to show. */
    public readonly contributor = input.required<ContributorDto>({ alias: 'sqxContributor' });

    constructor(
        private readonly contributorsState: TeamContributorsState,
    ) {
    }

    public remove() {
        this.contributorsState.revoke(this.contributor());
    }

    public changeRole(role: string) {
        const request = new AssignContributorDto({ contributorId: this.contributor().contributorId, role });

        this.contributorsState.assign(request);
    }
}
