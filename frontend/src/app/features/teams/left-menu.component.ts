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
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    imports: [
        RouterLink,
        RouterLinkActive,
        TranslatePipe,
    ],
})
export class LeftMenuComponent {
    /** The team to show the menu for. */
    public readonly team = input.required<TeamDto>();
}
