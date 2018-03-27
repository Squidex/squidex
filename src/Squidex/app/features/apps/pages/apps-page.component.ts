/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppContext,
    AppDto,
    AppsState,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    OnboardingService
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class AppsPageComponent implements OnDestroy, OnInit {
    private appsSubscription: Subscription;

    public addAppDialog = new ModalView();

    public apps: ImmutableArray<AppDto>;
    public apptemplate = '';

    public onboardingModal = new ModalView();

    constructor(
        public readonly ctx: AppContext,
        private readonly appsState: AppsState,
        private readonly onboardingService: OnboardingService
    ) {
    }

    public ngOnDestroy() {
        this.appsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsSubscription =
            this.appsState.apps
                .subscribe(apps => {
                    if (apps.length === 0 && this.onboardingService.shouldShow('dialog')) {
                        this.onboardingService.disable('dialog');
                        this.onboardingModal.show();
                    }

                    this.apps = apps;
                });
    }

    public createNewApp(template: string) {
        this.apptemplate = template;

        this.addAppDialog.show();
    }
}