/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ConfirmClickDirective, FileSizePipe, FormHintComponent, KNumberPipe, PlanInfo, PlansState, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-plan',
    styleUrls: ['./plan.component.scss'],
    templateUrl: './plan.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        FileSizePipe,
        FormHintComponent,
        KNumberPipe,
        TranslatePipe,
    ],
})
export class PlanComponent {
    @Input({ required: true })
    public planInfo!: PlanInfo;

    constructor(
        public readonly plansState: PlansState,
    ) {
    }

    public changeMonthly() {
        this.plansState.change(this.planInfo.plan.id);
    }

    public changeYearly() {
        this.plansState.change(this.planInfo.plan.yearlyId);
    }
}
