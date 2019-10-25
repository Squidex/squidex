/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import {
    fadeAnimation,
    PlanInfo,
    PlansState
} from '@app/shared';

@Component({
    selector: 'sqx-plan',
    styleUrls: ['./plan.component.scss'],
    templateUrl: './plan.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlanComponent {
    @Input()
    public planInfo: PlanInfo;

    constructor(
        public readonly plansState: PlansState
    ) {
    }

    public changeMonthly() {
        this.plansState.change(this.planInfo.plan.id);
    }

    public changeYearly() {
        this.plansState.change(this.planInfo.plan.yearlyId);
    }
}