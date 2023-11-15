/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ConfirmClickDirective, FileSizePipe, FormHintComponent, KNumberPipe, PlanInfo, TranslatePipe } from '@app/shared';
import { TeamPlansState } from '../../internal';

@Component({
    selector: 'sqx-plan',
    styleUrls: ['./plan.component.scss'],
    templateUrl: './plan.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        FormHintComponent,
        NgIf,
        ConfirmClickDirective,
        AsyncPipe,
        FileSizePipe,
        KNumberPipe,
        TranslatePipe,
    ],
})
export class PlanComponent {
    @Input({ required: true })
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
