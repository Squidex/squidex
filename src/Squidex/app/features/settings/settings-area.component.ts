/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    DialogService
} from 'shared';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html'
})
export class SettingsAreaComponent extends AppComponentBase {
    constructor(apps: AppsStoreService, dialogs: DialogService
    ) {
        super(dialogs, apps);
    }
}