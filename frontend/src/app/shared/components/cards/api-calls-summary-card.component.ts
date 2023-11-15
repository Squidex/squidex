/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CallsUsageDto, KNumberPipe, TranslatePipe } from '@app/framework';

@Component({
    selector: 'sqx-api-calls-summary-card',
    styleUrls: ['./api-calls-summary-card.component.scss'],
    templateUrl: './api-calls-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        KNumberPipe,
        TranslatePipe,
    ],
})
export class ApiCallsSummaryCardComponent {
    @Input({ required: true })
    public usage?: CallsUsageDto;

    public callsMonth = 0;
    public callsAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.callsMonth = this.usage.monthCalls;
            this.callsAllowed = this.usage.allowedCalls;
        }
    }
}
