/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppsState, LayoutContainerDirective, TitleComponent } from '@app/shared';
import { LeftMenuComponent } from './left-menu.component';

@Component({
    selector: 'sqx-app-area',
    styleUrls: ['./app-area.component.scss'],
    templateUrl: './app-area.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutContainerDirective,
        LeftMenuComponent,
        RouterOutlet,
        TitleComponent,
    ],
})
export class AppAreaComponent {
    protected readonly appsState = inject(AppsState);
}
