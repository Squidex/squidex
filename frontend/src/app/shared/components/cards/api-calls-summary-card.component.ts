/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { CallsUsageDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-calls-summary-card[usage]',
    styleUrls: ['./api-calls-summary-card.component.scss'],
    templateUrl: './api-calls-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiCallsSummaryCardComponent implements OnChanges {
    @Input()
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
