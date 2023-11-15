/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { NgFor } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, ContributorDto, ContributorsState, HighlightPipe, RoleDto, TooltipDirective, UserPicturePipe } from '@app/shared';

@Component({
    selector: '[sqxContributor][roles]',
    styleUrls: ['./contributor.component.scss'],
    templateUrl: './contributor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        TooltipDirective,
        FormsModule,
        NgFor,
        ConfirmClickDirective,
        HighlightPipe,
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
        this.contributorsState.assign({ contributorId: this.contributor.contributorId, role });
    }
}
