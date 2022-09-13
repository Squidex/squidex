/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ModalModel, TeamDto } from '@app/shared';

@Component({
    selector: 'sqx-team[team]',
    styleUrls: ['./team.component.scss'],
    templateUrl: './team.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamComponent {
    @Input()
    public team!: TeamDto;

    @Output()
    public leave = new EventEmitter<TeamDto>();

    public dropdown = new ModalModel();
}
