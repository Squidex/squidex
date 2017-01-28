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
    NotificationService,
    UsersProviderService
 } from 'shared';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html'
})
export class DashboardPageComponent extends AppComponentBase {
    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService) {
        super(apps, notifications, users);
    }
}

