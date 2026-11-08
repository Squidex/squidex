/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppsState, LayoutComponent, TitleComponent } from '@app/shared';
import { SettingsMenuComponent } from './settings-menu.component';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
    imports: [
        LayoutComponent,
        RouterOutlet,
        SettingsMenuComponent,
        TitleComponent,
    ],
})
export class SettingsAreaComponent {
    protected readonly appsState = inject(AppsState);
}
