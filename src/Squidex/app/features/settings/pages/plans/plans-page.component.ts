/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AppPlansDto,
    AppsState,
    AuthService,
    ChangePlanDto,
    DialogService,
    PlansService
} from '@app/shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html'
})
export class PlansPageComponent implements OnDestroy, OnInit {
    private queryParamsSubscription: Subscription;
    private overridePlanId: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    public plans: AppPlansDto;
    public planOwned = false;

    public isDisabled = false;

    constructor(
        public readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly plansService: PlansService,
        private readonly route: ActivatedRoute
    ) {
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

    public load(notifyLoad = false) {
        this.plansService.getPlans(this.appsState.appName)
            .subscribe(dto => {
                if (this.overridePlanId) {
                    this.plans = dto.changePlanId(this.overridePlanId);
                } else {
                    this.plans = dto;
                }

                this.planOwned = !dto.planOwner || (dto.planOwner === this.authState.user!.id);

                if (notifyLoad) {
                    this.dialogs.notifyInfo('Plans reloaded.');
                }
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public changePlan(planId: string) {
        this.isDisabled = true;

        this.plansService.putPlan(this.appsState.appName, new ChangePlanDto(planId), this.plans.version)
            .do(() => {
                this.isDisabled = false;
            })
            .subscribe(dto => {
                if (dto.payload.redirectUri && dto.payload.redirectUri.length > 0) {
                    window.location.href = dto.payload.redirectUri;
                } else {
                    this.plans = this.plans.changePlanId(planId, dto.version);
                }
            }, error => {
                this.dialogs.notifyError(error);
            });
    }
}

