/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import { AppsStoreService, NotificationService } from './../declarations-base';

import { ComponentBase } from './component-base';

export abstract class AppComponentBase extends ComponentBase {
    constructor(notifications: NotificationService,
        private readonly appsStore: AppsStoreService
    ) {
        super(notifications);
    }

    public appName(): Observable<string> {
        return this.appsStore.selectedApp.map(a => a!.name).take(1);
    }
}

