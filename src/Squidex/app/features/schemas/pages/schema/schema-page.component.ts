/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    NotificationService,
    SchemasService,
    UsersProviderService
} from 'shared';

const FALLBACK_NAME = 'my-schema';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html'
})
export class SchemaPageComponent extends AppComponentBase {
    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService
    ) {
        super(apps, notifications, users);
    }
}

