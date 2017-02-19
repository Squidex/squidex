/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import {
    AppsStoreService,
    NotificationService,
    UsersProviderService
} from 'shared';

import { ComponentBase } from './component-base';

export abstract class AppComponentBase extends ComponentBase {
    constructor(notifications: NotificationService, users: UsersProviderService,
        private readonly appsStore: AppsStoreService
    ) {
        super(notifications, users);
    }

    public appName(): Observable<string> {
        return this.appsStore.selectedApp.map(a => a!.name).take(1);
    }
}

