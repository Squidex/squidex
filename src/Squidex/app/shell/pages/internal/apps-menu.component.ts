/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

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
export class AppsMenuComponent implements OnDestroy, OnInit {
    private appsSubscription: Subscription;
    private appSubscription: Subscription;

    public modalMenu = new ModalView(false, true);
    public modalDialog = new ModalView();

    public apps: AppDto[] = [];

    public appName = FALLBACK_NAME;

    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsStore.apps.subscribe(apps => {
                this.apps = apps;
            });

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(selectedApp => this.appName = selectedApp ? selectedApp.name : FALLBACK_NAME);
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}