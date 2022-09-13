/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TeamPlansState } from '@app/features/teams/internal';
import { ApiUrlConfig, PlanDto } from '@app/shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
})
export class PlansPageComponent implements OnInit {
    private overridePlanId?: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    constructor(
        public readonly plansState: TeamPlansState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.overridePlanId = this.route.snapshot.queryParams['planId'];

        this.plansState.load(false, this.overridePlanId);
    }

    public reload() {
        this.plansState.load(true, this.overridePlanId);
    }

    public trackByPlan(_index: number, planInfo: { plan: PlanDto }) {
        return planInfo.plan.id;
    }
}
