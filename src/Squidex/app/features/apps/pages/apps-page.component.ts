/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription, timer } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    BackupsService,
    DialogModel,
    DialogService,
    ModalModel,
    OnboardingService,
    RestoreDto,
    RestoreForm
} from '@app/shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html'
})
export class AppsPageComponent implements OnDestroy, OnInit {
    private timerSubscription: Subscription;

    public addAppDialog = new DialogModel();
    public addAppTemplate = '';

    public restoreJob: RestoreDto | null;
    public restoreForm = new RestoreForm(this.formBuilder);

    public onboardingModal = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        private readonly onboardingService: OnboardingService
    ) {
    }

    public ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.timerSubscription =
            timer(0, 3000).pipe(switchMap(t => this.backupsService.getRestore()))
                .subscribe(dto => {
                    this.restoreJob = dto;
                });

        this.appsState.apps.pipe(
                take(1))
            .subscribe(apps => {
                if (this.onboardingService.shouldShow('dialog') && apps.length === 0) {
                    this.onboardingService.disable('dialog');
                    this.onboardingModal.show();
                }
            });
    }

    public restore() {
        const value = this.restoreForm.submit();

        if (value) {
            this.restoreForm.submitCompleted({});

            this.backupsService.postRestore(value.url)
                .subscribe(() => {
                    this.dialogs.notifyInfo('Restore started, it can take several minutes to complete.');
                }, error => {
                    this.dialogs.notifyError(error);
                });
        }
    }

    public createNewApp(template: string) {
        this.addAppTemplate = template;
        this.addAppDialog.show();
    }
}