/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { CallsUsageDto, FileSizePipe } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-traffic-summary-card',
    styleUrls: ['./api-traffic-summary-card.component.scss'],
    templateUrl: './api-traffic-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        FileSizePipe,
        TranslatePipe,
    ],
})
export class ApiTrafficSummaryCardComponent {
    @Input({ required: true })
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
