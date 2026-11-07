/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { TranslatePipe } from '@app/framework';
import { AppDto, CallsUsageDto, ChartHelpers, ChartOptions, UsagesService } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-calls-card',
    styleUrls: ['./api-calls-card.component.scss'],
    templateUrl: './api-calls-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        BaseChartDirective,
        TranslatePipe,
    ],
})
export class ApiCallsCardComponent {
    private readonly usagesService = inject(UsagesService);

    /** The app to download the usage log for. Hides the download link when not set. */
    public readonly app = input<AppDto | null>();

    /** The API usage of the app or team. */
    public readonly usage = input.required<CallsUsageDto | undefined>();

    protected readonly chartData = computed<ChartData | undefined>(() => {
        const usage = this.usage();
        if (!usage) {
            return undefined;
        }

        return {
            labels: [...ChartHelpers.createLabelsFromSet(usage.details)],
            datasets: Object.entries(usage.details).map(([key, value], i) => (
                {
                    label: ChartHelpers.label(key),
                    backgroundColor: ChartHelpers.getBackgroundColor(i),
                    borderColor: ChartHelpers.getBorderColor(i),
                    borderWidth: 1,
                    data: value.map(x => x.totalCalls),
                })),
        };
    });

    protected readonly chartOptions = ChartOptions.Stacked;

    protected downloadLog(app: AppDto) {
        this.usagesService.getLog(app.name)
            .subscribe(url => {
                window.open(url, '_blank');
            });
    }
}
