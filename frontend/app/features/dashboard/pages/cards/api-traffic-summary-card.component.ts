
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { AppDto, CallsUsageDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-api-traffic-summary-card',
    styleUrls: ['./api-traffic-summary-card.component.scss'],
    templateUrl: './api-traffic-summary-card.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ApiTrafficSummaryCardComponent implements OnChanges {
    @Input()
    public app: AppDto;

    @Input()
    public usage: CallsUsageDto;

    public bytesMonth = 0;
    public bytesAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.bytesMonth = this.usage.monthBytes;
            this.bytesAllowed = this.usage.allowedBytes;
        }
    }
}