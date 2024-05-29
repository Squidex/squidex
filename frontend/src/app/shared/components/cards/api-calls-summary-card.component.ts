/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { KNumberPipe, TranslatePipe } from '@app/framework';
import { CallsUsageDto } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-api-calls-summary-card',
    styleUrls: ['./api-calls-summary-card.component.scss'],
    templateUrl: './api-calls-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
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
