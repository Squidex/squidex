/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutComponent, TeamsState, TitleComponent } from '@app/shared';
import { SettingsMenuComponent } from './settings-menu.component';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutComponent,
        RouterOutlet,
        SettingsMenuComponent,
        TitleComponent,
    ],
})
export class SettingsAreaComponent {
    protected readonly teamsState = inject(TeamsState);
}
