/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { CallsUsageDto, ChartHelpers, ChartOptions } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-performance-card[usage]',
    styleUrls: ['./api-performance-card.component.scss'],
    templateUrl: './api-performance-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiPerformanceCardComponent implements OnChanges {
    @Input()
    public usage?: CallsUsageDto;

    @Input()
    public isStacked?: boolean | null;

    @Output()
    public isStackedChange = new EventEmitter<boolean>();

    public chartData: any;
    public chartSummary = 0;

    public get chartOptions() {
        return this.isStacked ? ChartOptions.Stacked : ChartOptions.Default;
    }

    public ngOnChanges() {
        if (this.usage) {
            const labels = ChartHelpers.createLabelsFromSet(this.usage.details);

            this.chartData = {
                labels,
                datasets: Object.entries(this.usage.details).map(([key, value], i) => (
                    {
                        label: ChartHelpers.label(key),
                        backgroundColor: ChartHelpers.getBackgroundColor(i),
                        borderColor: ChartHelpers.getBorderColor(i),
                        borderWidth: 1,
                        data: value.map(x => x.averageElapsedMs),
                    })),
            };

            this.chartSummary = this.usage.averageElapsedMs;
        }
    }
}
