/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

import { AppsStoreService, LocalStoreService } from 'shared';

@Component({
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html'
})
export class LeftMenuComponent implements OnInit, OnDestroy {
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