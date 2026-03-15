/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
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
    @Input()
    public roles!: ReadonlyArray<RoleDto>;

    @Input()
    public search?: string | RegExp | null;

    @Input('sqxContributor')
    public contributor!: ContributorDto;

    constructor(
        private readonly contributorsState: ContributorsState,
    ) {
    }

    public remove() {
        this.contributorsState.revoke(this.contributor);
    }

    public changeRole(role: string) {
        const request = new AssignContributorDto({ contributorId: this.contributor.contributorId, role });

        this.contributorsState.assign(request);
    }
}
