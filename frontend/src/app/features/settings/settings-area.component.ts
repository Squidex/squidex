/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppsState, defined, LayoutComponent, TitleComponent } from '@app/shared';
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
    public selectedApp = this.appsState.selectedApp.pipe(defined());

    constructor(
        private readonly appsState: AppsState,
    ) {
    }
}
