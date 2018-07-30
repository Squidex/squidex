/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    DialogModel,
    ModalModel,
    OnboardingService
} from '@app/shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html'
})
export class AppsPageComponent implements OnInit {
    public addAppDialog = new DialogModel();
    public addAppTemplate = '';

    public onboardingModal = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly onboardingService: OnboardingService
    ) {
    }

    public ngOnInit() {
        this.appsState.apps.pipe(
                take(1))
            .subscribe(apps => {
                if (this.onboardingService.shouldShow('dialog') && apps.length === 0) {
                    this.onboardingService.disable('dialog');
                    this.onboardingModal.show();
                }
            });
    }

    public createNewApp(template: string) {
        this.addAppTemplate = template;
        this.addAppDialog.show();
    }
}