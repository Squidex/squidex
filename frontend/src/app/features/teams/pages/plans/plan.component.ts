/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TeamPlansState } from '@app/features/teams/internal';
import { PlanInfo } from '@app/shared';

@Component({
    selector: 'sqx-plan[planInfo]',
    styleUrls: ['./plan.component.scss'],
    templateUrl: './plan.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlanComponent {
    @Input()
    public planInfo!: PlanInfo;

    constructor(
        public readonly plansState: TeamPlansState,
    ) {
    }

    public changeMonthly() {
        this.plansState.change(this.planInfo.plan.id);
    }

    public changeYearly() {
        this.plansState.change(this.planInfo.plan.yearlyId);
    }
}
