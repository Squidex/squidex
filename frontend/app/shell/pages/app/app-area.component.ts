/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { AppsState } from '@app/shared';

@Component({
    selector: 'sqx-app-area',
    styleUrls: ['./app-area.component.scss'],
    templateUrl: './app-area.component.html'
})
export class AppAreaComponent {
    constructor(
        public readonly appsState: AppsState
    ) {
    }
}