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
    AuthService,
    ImmutableArray,
    ModalView,
    OnboardingService
} from '@app/shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html'
})
export class AppsPageComponent implements OnDestroy, OnInit {
    private appsSubscription: Subscription;

    public addAppDialog = new ModalView();

    public apps: ImmutableArray<AppDto> = ImmutableArray.empty();
    public appTemplate = '';

    public onboardingModal = new ModalView();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
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
        this.appTemplate = template;

        this.addAppDialog.show();
    }
}