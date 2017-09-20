/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AppComponentBase,
    AppPlansDto,
    AppsStoreService,
    AuthService,
    ChangePlanDto,
    DialogService,
    PlansService
} from 'shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html'
})
export class PlansPageComponent extends AppComponentBase implements OnDestroy, OnInit {
    private queryParamsSubscription: Subscription;
    private overridePlanId: string;

    public portalUrl = this.apiUrl.buildUrl('/identity-server/account/portal');

    public plans: AppPlansDto;
    public planOwned = false;

    public isDisabled = false;

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly plansService: PlansService,
        private readonly route: ActivatedRoute,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnDestroy() {
        this.queryParamsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.queryParamsSubscription =
            this.route.queryParams.subscribe(params => {
                this.overridePlanId = params['planId'];
            });

        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app => this.plansService.getPlans(app).retry(2))
            .subscribe(dto => {
                if (this.overridePlanId) {
                    this.plans = dto.changePlanId(this.overridePlanId);
                } else {
                    this.plans = dto;
                }

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
            .switchMap(app => this.plansService.putPlan(app, new ChangePlanDto(planId), this.plans.version))
            .subscribe(dto => {
                if (dto.payload.redirectUri && dto.payload.redirectUri.length > 0) {
                    window.location.href = dto.payload.redirectUri;
                } else {
                    this.plans = this.plans.changePlanId(planId, dto.version);
                    this.isDisabled = false;
                }
            }, error => {
                this.notifyError(error);
                this.isDisabled = false;
            });
    }
}

