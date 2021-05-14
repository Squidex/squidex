/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { AppsState, defined } from '@app/shared';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
})
export class SettingsAreaComponent {
    public selectedApp = this.appsState.selectedApp.pipe(defined());

    constructor(
        private readonly appsState: AppsState,
    ) {
    }
}
