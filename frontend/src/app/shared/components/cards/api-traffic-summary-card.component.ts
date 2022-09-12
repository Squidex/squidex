/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { CallsUsageDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-traffic-summary-card[usage]',
    styleUrls: ['./api-traffic-summary-card.component.scss'],
    templateUrl: './api-traffic-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiTrafficSummaryCardComponent implements OnChanges {
    @Input()
    public usage?: CallsUsageDto;

    public bytesMonth = 0;
    public bytesAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.bytesMonth = this.usage.monthBytes;
            this.bytesAllowed = this.usage.allowedBytes;
        }
    }
}
