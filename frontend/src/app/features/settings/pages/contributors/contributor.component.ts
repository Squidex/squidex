/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, input, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AssignContributorDto, ConfirmClickDirective, ContributorDto, ContributorsState, HighlightPipe, RoleDto, TooltipDirective, TranslatePipe, UserPicturePipe } from '@app/shared';

@Component({
    selector: '[sqxContributor][roles]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FormsModule,
        HighlightPipe,
        TooltipDirective,
        TranslatePipe,
        UserPicturePipe,
    ],
})
export class ContributorComponent {
    private readonly contributorsState = inject(ContributorsState);

    /** The roles that can be assigned to the contributor. */
    public readonly roles = input.required<ReadonlyArray<RoleDto>>();

    /** The search term to highlight in the contributor name. */
    public readonly search = input<string | RegExp | null>();

    /** The contributor to show. */
    public readonly contributor = input.required<ContributorDto>({ alias: 'sqxContributor' });

    protected remove() {
        this.contributorsState.revoke(this.contributor());
    }

    protected changeRole(role: string) {
        const request = new AssignContributorDto({ contributorId: this.contributor().contributorId, role });

        this.contributorsState.assign(request);
    }
}
