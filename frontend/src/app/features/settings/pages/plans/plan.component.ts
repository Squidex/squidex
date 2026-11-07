/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { ConfirmClickDirective, FileSizePipe, FormHintComponent, KNumberPipe, PlanInfo, PlansState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-plan',
    styleUrls: ['./plan.component.scss'],
    templateUrl: './plan.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FileSizePipe,
        FormHintComponent,
        KNumberPipe,
        TranslatePipe,
    ],
})
export class PlanComponent {
    protected readonly plansState = inject(PlansState);

    /** The plan to show, including whether it is currently selected. */
    public readonly planInfo = input.required<PlanInfo>();

    protected changeMonthly() {
        this.plansState.change(this.planInfo().plan.id);
    }

    protected changeYearly() {
        this.plansState.change(this.planInfo().plan.yearlyId!);
    }
}
