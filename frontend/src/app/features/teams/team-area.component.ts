/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutContainerDirective, TeamsState, TitleComponent } from '@app/shared';
import { LeftMenuComponent } from './left-menu.component';

@Component({
    selector: 'sqx-team-area',
    styleUrls: ['./team-area.component.scss'],
    templateUrl: './team-area.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutContainerDirective,
        LeftMenuComponent,
        RouterOutlet,
        TitleComponent,
    ],
})
export class TeamAreaComponent {
    protected readonly teamsState = inject(TeamsState);
}
