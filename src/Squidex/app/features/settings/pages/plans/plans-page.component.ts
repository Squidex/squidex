/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AppContext,
    AppPlansDto,
    ChangePlanDto,
    PlansService
} from 'shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
    providers: [
        AppContext
    ]
})
export class PlansPageComponent implements OnDestroy, OnInit {
    private queryParamsSubscription: Subscription;
    private overridePlanId: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    public plans: AppPlansDto;
    public planOwned = false;

    public isDisabled = false;

    constructor(public readonly ctx: AppContext,
        private readonly plansService: PlansService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.queryParamsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.queryParamsSubscription =
            this.ctx.route.queryParams.subscribe(params => {
                this.overridePlanId = params['planId'];
            });

        this.load();
    }

    public load(showInfo = false) {
        this.plansService.getPlans(this.ctx.appName)
            .subscribe(dto => {
                if (this.overridePlanId) {
                    this.plans = dto.changePlanId(this.overridePlanId);
                } else {
                    this.plans = dto;
                }

                this.planOwned = !dto.planOwner || (dto.planOwner === this.ctx.userId);

                if (showInfo) {
                    this.ctx.notifyInfo('Plans reloaded.');
                }
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public changePlan(planId: string) {
        this.isDisabled = true;

        this.plansService.putPlan(this.ctx.appName, new ChangePlanDto(planId), this.plans.version)
            .subscribe(dto => {
                if (dto.payload.redirectUri && dto.payload.redirectUri.length > 0) {
                    window.location.href = dto.payload.redirectUri;
                } else {
                    this.plans = this.plans.changePlanId(planId, dto.version);
                    this.isDisabled = false;
                }
            }, error => {
                this.ctx.notifyError(error);
                this.isDisabled = false;
            });
    }
}

