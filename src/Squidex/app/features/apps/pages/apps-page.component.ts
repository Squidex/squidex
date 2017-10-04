/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppsStoreService,
    fadeAnimation,
    ModalView,
    OnboardingService
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsPageComponent implements OnDestroy, OnInit {
    private onboardingAppsSubscription: Subscription;

    public addAppDialog = new ModalView();
    public apps = this.appsStore.apps;

    public onboardingModal = new ModalView();

    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly onboardingService: OnboardingService
    ) {
    }

    public ngOnDestroy() {
        this.onboardingAppsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);

        this.onboardingAppsSubscription =
            this.appsStore.apps
                .subscribe(apps => {
                    if (apps.length === 0 && this.onboardingService.shouldShow('dialog')) {
                        this.onboardingService.disable('dialog');
                        this.onboardingModal.show();
                    }
                });
    }
}