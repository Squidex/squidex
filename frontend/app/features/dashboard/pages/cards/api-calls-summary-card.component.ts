/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { AppDto, CallsUsageDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-api-calls-summary-card[app][usage]',
    styleUrls: ['./api-calls-summary-card.component.scss'],
    templateUrl: './api-calls-summary-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiCallsSummaryCardComponent implements OnChanges {
    @Input()
    public app: AppDto;

    @Input()
    public usage: CallsUsageDto;

    public callsMonth = 0;
    public callsAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.callsMonth = this.usage.monthCalls;
            this.callsAllowed = this.usage.allowedCalls;
        }
    }
}
