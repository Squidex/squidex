/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { CallsUsageDto, FileSizePipe } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-traffic-summary-card',
    styleUrls: ['./api-traffic-summary-card.component.scss'],
    templateUrl: './api-traffic-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FileSizePipe,
        TranslatePipe,
    ],
})
export class ApiTrafficSummaryCardComponent {
    /** The API usage of the app. */
    public readonly usage = input.required<CallsUsageDto | undefined>();

    protected readonly bytesMonth = computed(() => {
        return this.usage()?.monthBytes ?? 0;
    });

    protected readonly bytesAllowed = computed(() => {
        return this.usage()?.allowedBytes ?? 0;
    });
}