
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

    public bytesTotal = 0;
    public bytesAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.bytesTotal = this.usage.totalBytes;
            this.bytesAllowed = this.usage.allowedBytes;
        }
    }
}