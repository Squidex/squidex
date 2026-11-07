/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input, inject } from '@angular/core';
import { ConfirmClickDirective, FileSizePipe, FormHintComponent, KNumberPipe, PlanInfo, TranslatePipe } from '@app/shared';
import { TeamPlansState } from '../../internal';

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
    protected readonly plansState = inject(TeamPlansState);

    /** The plan to show, including whether it is selected. */
    public readonly planInfo = input.required<PlanInfo>();

    protected changeMonthly() {
        this.plansState.change(this.planInfo().plan.id);
    }

    protected changeYearly() {
        this.plansState.change(this.planInfo().plan.yearlyId!);
    }
}
