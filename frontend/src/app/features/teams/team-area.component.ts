/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { defined, LayoutContainerDirective, TeamsState, TitleComponent } from '@app/shared';
import { LeftMenuComponent } from './left-menu.component';

@Component({
    standalone: true,
    selector: 'sqx-team-area',
    styleUrls: ['./team-area.component.scss'],
    templateUrl: './team-area.component.html',
    imports: [
        AsyncPipe,
        LayoutContainerDirective,
        LeftMenuComponent,
        RouterOutlet,
        TitleComponent,
    ],
})
export class TeamAreaComponent {
    public selectedTeam = this.teamsState.selectedTeam.pipe(defined());

    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }
}
