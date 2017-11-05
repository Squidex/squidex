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
    AppsService,
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
    private appsSubscription: Subscription;

    public addAppDialog = new ModalView();
    public apps: AppDto[];

    public onboardingModal = new ModalView();

    constructor(
        private readonly appsService: AppsService,
        private readonly onboardingService: OnboardingService
    ) {
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsService.getApps()
                .subscribe(apps => {
                    if (apps.length === 0 && this.onboardingService.shouldShow('dialog')) {
                        this.onboardingService.disable('dialog');
                        this.onboardingModal.show();
                    }

                    this.apps = apps;
                });
    }
}