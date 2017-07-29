/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    ApiUrlConfig,
    AppComponentBase,
    AppPlansDto,
    AppsStoreService,
    AuthService,
    ChangePlanDto,
    FileHelper,
    NotificationService,
    PlansService,
    Version
} from 'shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html'
})
export class PlansPageComponent extends AppComponentBase implements OnInit {
    private version = new Version();

    public portalUrl = this.apiUrl.buildUrl('/identity-server/account/portal');

    public plans: AppPlansDto;
    public planOwned = false;

    public isDisabled = false;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly authService: AuthService,
        private readonly plansService: PlansService,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app => this.plansService.getPlans(app, this.version).retry(2))
            .subscribe(dto => {
                this.plans = dto;
                this.planOwned = !dto.planOwner || (dto.planOwner === this.authService.user!.id);

                if (showInfo) {
                    this.notifyInfo('Plans reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public changePlan(planId: string) {
        this.isDisabled = true;

        this.appNameOnce()
            .switchMap(app => this.plansService.putPlan(app, new ChangePlanDto(planId), this.version))
            .subscribe(dto => {
                this.plans =
                    new AppPlansDto(planId,
                         this.plans.planOwner,
                         this.plans.hasPortal,
                         this.plans.hasConfigured,
                         this.plans.plans);
            }, error => {
                this.notifyError(error);
            }, () => {
                this.isDisabled = false;
            });
    }

    public formatSize(count: number): string {
        return FileHelper.fileSize(count);
    }

    public formatCalls(count: number): string | null {
        if (count > 1000) {
            count = count / 1000;

            if (count < 10) {
                count = Math.round(count * 10) / 10;
            } else {
                count = Math.round(count);
            }

            return count + 'k';
        } else if (count < 0) {
            return null;
        } else {
            return count.toString();
        }
    }
}

