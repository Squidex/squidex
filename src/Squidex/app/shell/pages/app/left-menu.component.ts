/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { AppsStoreService, LocalStoreService } from 'shared';

@Ng2.Component({
    selector: 'sqx-left-menu',
    styles,
    template
})
export class LeftMenuComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private appSubscription: any | null = null;

    public get showSettingsMenu(): boolean {
        return this.localStore.get('squidex:showSettingsMenu') === 'true';
    }

    public set showSettingsMenu(value: boolean) {
        this.localStore.set('squidex:showSettingsMenu', value);
    }

    public permission: string | null = null;

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.permission = app.permission;
                }
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public toggleSettingsMenu() {
        this.showSettingsMenu = !this.showSettingsMenu;
    }
}