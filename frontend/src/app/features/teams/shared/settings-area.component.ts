/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { defined, LayoutComponent, TeamsState, TitleComponent } from '@app/shared';
import { SettingsMenuComponent } from './settings-menu.component';

@Component({
    standalone: true,
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
    imports: [
        AsyncPipe,
        LayoutComponent,
        RouterOutlet,
        SettingsMenuComponent,
        TitleComponent,
    ],
})
export class SettingsAreaComponent {
    public selectedTeam = this.teamsState.selectedTeam.pipe(defined());

    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }
}
