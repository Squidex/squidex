/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import {
    AppsStoreService,
    AuthService,
    DialogService
} from './../declarations-base';

import { ComponentBase } from './component-base';

export abstract class AppComponentBase extends ComponentBase {
    private appName$: Observable<string>;

    public get userToken(): string {
        return this.authService.user!.token;
    }

    constructor(dialogs: DialogService,
        protected readonly appsStore: AppsStoreService,
        protected readonly authService: AuthService
    ) {
        super(dialogs);

        this.appName$ = this.appsStore.selectedApp.filter(a => !!a).map(a => a!.name);
    }

    public appName(): Observable<string> {
        return this.appName$;
    }

    public appNameOnce(): Observable<string> {
        return this.appName$.first();
    }
}

