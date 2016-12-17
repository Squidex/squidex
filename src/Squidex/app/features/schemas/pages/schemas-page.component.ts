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
    fadeAnimation,
    ModalView,
    NotificationService,
    UsersProviderService
 } from 'shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent extends AppComponentBase {
    public modalDialog = new ModalView();

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService) {
        super(apps, notifications, users);
    }

    public createSchema() {
        this.modalDialog.show();
    }
}

