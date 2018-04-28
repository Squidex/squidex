/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import { AppsState } from '@app/shared';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html'
})
export class SettingsAreaComponent {
    constructor(
        public readonly appsState: AppsState
    ) {
    }
}