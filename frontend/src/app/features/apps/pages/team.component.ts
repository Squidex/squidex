/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, TeamDto, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-team',
    styleUrls: ['./team.component.scss'],
    templateUrl: './team.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    @Input({ required: true })
    public team!: TeamDto;

    @Output()
    public leave = new EventEmitter<TeamDto>();

    public dropdown = new ModalModel();
}
