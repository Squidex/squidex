/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, TeamDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-team',
    styleUrls: ['./team.component.scss'],
    templateUrl: './team.component.html',
    imports: [
        ConfirmClickDirective,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class TeamComponent {
    /** The team to show in the header. */
    public readonly team = input.required<TeamDto>();

    /** Emits the team when the user confirms to leave it. */
    public readonly leave = output<TeamDto>();

    protected readonly dropdown = new ModalModel();
}
