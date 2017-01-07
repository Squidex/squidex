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
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html'
})
export class AppsPageComponent implements OnInit, OnDestroy {
    private appsSubscription: Subscription;

    public modalDialog = new ModalView();

    public apps: AppDto[];

    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);

        this.appsSubscription =
            this.appsStore.apps.subscribe(apps => {
                this.apps = apps || [];
            });
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
    }
}