/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppDto,
    AppsState,
    ImmutableArray,
    fadeAnimation,
    ModalView
} from '@app/shared';

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

    public addAppDialog = new ModalView();

    public appsMenu = new ModalView(false, true);
    public apps = ImmutableArray.of<AppDto>([]);

    public selectedApp: AppDto | null;

    constructor(
        private readonly appsStore: AppsState
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
            this.appsStore.selectedApp.subscribe(app => {
                this.selectedApp = app;
            });
    }

    public createApp() {
        this.appsMenu.hide();
        this.addAppDialog.show();
    }
}