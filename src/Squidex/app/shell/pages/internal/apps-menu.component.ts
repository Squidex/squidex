/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppContext,
    AppDto,
    AppsService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    providers: [
        AppContext
    ],
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

    constructor(public readonly ctx: AppContext,
        private readonly appsService: AppsService
    ) {
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsService.getApps().subscribe(apps => {
                this.apps = apps;
            });
    }

    public createApp() {
        this.modalMenu.hide();
        this.modalDialog.show();
    }
}