/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { defined, LayoutContainerDirective, TeamsState, TitleComponent } from '@app/shared';
import { LeftMenuComponent } from './left-menu.component';

@Component({
    selector: 'sqx-team-area',
    styleUrls: ['./team-area.component.scss'],
    templateUrl: './team-area.component.html',
    standalone: true,
    imports: [
        NgIf,
        TitleComponent,
        LeftMenuComponent,
        LayoutContainerDirective,
        RouterOutlet,
        AsyncPipe,
    ],
})
export class TeamAreaComponent {
    public selectedTeam = this.teamsState.selectedTeam.pipe(defined());

    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }
}
