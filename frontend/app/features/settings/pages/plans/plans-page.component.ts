/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiUrlConfig, PlanDto, PlansState } from '@app/shared';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
})
export class PlansPageComponent implements OnInit {
    private overridePlanId: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    constructor(
        public readonly plansState: PlansState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.route.queryParams
            .subscribe(params => {
                this.overridePlanId = params['planId'];
            }).unsubscribe();

        this.plansState.load(false, this.overridePlanId);
    }

    public reload() {
        this.plansState.load(true, this.overridePlanId);
    }

    public trackByPlan(_index: number, planInfo: { plan: PlanDto }) {
        return planInfo.plan.id;
    }
}
