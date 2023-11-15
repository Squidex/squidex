/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { defined, LayoutComponent, TeamsState, TitleComponent } from '@app/shared';
import { SettingsMenuComponent } from './settings-menu.component';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        NgIf,
        SettingsMenuComponent,
        RouterOutlet,
        AsyncPipe,
    ],
})
export class SettingsAreaComponent {
    public selectedTeam = this.teamsState.selectedTeam.pipe(defined());

    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }
}
