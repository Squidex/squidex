/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { TranslatePipe } from '@app/framework';
import { CallsUsageDto, ChartHelpers, ChartOptions } from '@app/shared';

@Component({
    selector: 'sqx-api-performance-card',
    styleUrls: ['./api-performance-card.component.scss'],
    templateUrl: './api-performance-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        BaseChartDirective,
        FormsModule,
        TranslatePipe,
    ],
})
export class ApiPerformanceCardComponent {
    /** The API usage of the app. */
    public readonly usage = input.required<CallsUsageDto | undefined>();

    /** Stacks the bars of the different clients. */
    public readonly isStacked = input(false, { transform: booleanAttribute });

    /** Emits when the user toggles the stacked mode. */
    public readonly isStackedChange = output<boolean>();

    public readonly chartOptions = computed(() => {
        return this.isStacked() ? ChartOptions.Stacked : ChartOptions.Default;
    });

    public readonly chartSummary = computed(() => {
        return this.usage()?.averageElapsedMs ?? 0;
    });

    public readonly chartData = computed<ChartData | undefined>(() => {
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
                    data: value.map(x => x.averageElapsedMs),
                })),
        };
    });
}
