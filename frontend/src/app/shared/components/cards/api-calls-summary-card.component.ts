/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { KNumberPipe, TranslatePipe } from '@app/framework';
import { CallsUsageDto } from '@app/shared';

@Component({
    selector: 'sqx-api-calls-summary-card',
    styleUrls: ['./api-calls-summary-card.component.scss'],
    templateUrl: './api-calls-summary-card.component.html',
    imports: [
        KNumberPipe,
        TranslatePipe,
    ],
})
export class ApiCallsSummaryCardComponent {
    /** The API usage of the app. */
    public readonly usage = input.required<CallsUsageDto | undefined>();

    protected readonly callsMonth = computed(() => {
        return this.usage()?.monthCalls ?? 0;
    });

    protected readonly callsAllowed = computed(() => {
        return this.usage()?.allowedCalls ?? 0;
    });
}