/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TeamDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-settings-menu',
    styleUrls: ['./settings-menu.component.scss'],
    templateUrl: './settings-menu.component.html',
    imports: [
        RouterLink,
        RouterLinkActive,
        TranslatePipe,
    ],
})
export class SettingsMenuComponent {
    /** The team to show the settings menu for. */
    public readonly team = input.required<TeamDto>();
}
