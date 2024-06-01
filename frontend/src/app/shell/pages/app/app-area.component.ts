/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppsState, defined, LayoutContainerDirective, TitleComponent } from '@app/shared';
import { LeftMenuComponent } from './left-menu.component';

@Component({
    standalone: true,
    selector: 'sqx-app-area',
    styleUrls: ['./app-area.component.scss'],
    templateUrl: './app-area.component.html',
    imports: [
        AsyncPipe,
        LayoutContainerDirective,
        LeftMenuComponent,
        RouterOutlet,
        TitleComponent,
    ],
})
export class AppAreaComponent {
    public selectedApp = this.appsState.selectedApp.pipe(defined());

    constructor(
        private readonly appsState: AppsState,
    ) {
    }
}
