/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
    standalone: true,
    selector: 'sqx-teams-area',
    styleUrls: ['./teams-area.component.scss'],
    templateUrl: './teams-area.component.html',
    imports: [
        RouterOutlet,
    ],
})
export class TeamsAreaComponent {
}
