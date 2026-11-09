/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { Component, inject, input } from '@angular/core';
import { ConfirmClickDirective, ContributorDto, HighlightPipe, TooltipDirective, TranslatePipe, UserPicturePipe } from '@app/shared';
import { TeamContributorsState } from '../../internal';

@Component({
    selector: '[sqxContributor]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    imports: [
        ConfirmClickDirective,
        HighlightPipe,
        TooltipDirective,
        TranslatePipe,
        UserPicturePipe,
    ],
})
export class ContributorComponent {
    private readonly contributorsState = inject(TeamContributorsState);

    /** The search term to highlight in the name of the contributor. */
    public readonly search = input<string | RegExp | null>();

    /** The contributor to show. */
    public readonly contributor = input.required<ContributorDto>({ alias: 'sqxContributor' });

    protected remove() {
        this.contributorsState.revoke(this.contributor());
    }
}
