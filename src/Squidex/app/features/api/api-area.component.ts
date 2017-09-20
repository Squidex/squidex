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
    AuthService,
    DialogService
} from 'shared';

@Component({
    selector: 'sqx-api-area',
    styleUrls: ['./api-area.component.scss'],
    templateUrl: './api-area.component.html'
})
export class ApiAreaComponent extends AppComponentBase {
    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService
    ) {
        super(dialogs, apps, authService);
    }
}