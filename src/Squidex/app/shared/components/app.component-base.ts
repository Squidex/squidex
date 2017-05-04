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
    private appName$: Observable<string>;

    constructor(notifications: NotificationService,
        private readonly appsStore: AppsStoreService
    ) {
        super(notifications);

        this.appName$ = this.appsStore.selectedApp.filter(a => !!a).map(a => a!.name);
    }

    public appName(): Observable<string> {
        return this.appName$;
    }

    public appNameOnce(): Observable<string> {
        return this.appName$.take(1);
    }
}

