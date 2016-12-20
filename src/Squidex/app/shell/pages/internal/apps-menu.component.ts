/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

import {
    AppDto,
    AppsStoreService,
    fadeAnimation,
    ModalView
} from 'shared';

const FALLBACK_NAME = 'Apps Overview';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsMenuComponent implements OnInit, OnDestroy {
    private appsSubscription: any | null = null;
    private appSubscription: any | null = null;

    public modalMenu = new ModalView(false, true);
    public modalDialog = new ModalView();

    public apps: AppDto[] = [];

    public appName = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsStore.apps.subscribe(apps => {
                this.apps = apps || [];
            });

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(selectedApp => this.appName = selectedApp ? selectedApp.name : FALLBACK_NAME);
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
        this.appSubscription.unsubscribe();
    }

    public onAppCreationCancelled() {
        this.modalDialog.hide();
    }

    public onAppCreationCompleted(app: AppDto) {
        this.modalDialog.hide();
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}