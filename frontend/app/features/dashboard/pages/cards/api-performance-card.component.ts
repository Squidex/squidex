/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { AppDto, CallsUsageDto, fadeAnimation } from '@app/shared';
import { ChartHelpers, ChartOptions } from './shared';

@Component({
    selector: 'sqx-api-performance-card[app][usage]',
    styleUrls: ['./api-performance-card.component.scss'],
    templateUrl: './api-performance-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiPerformanceCardComponent implements OnChanges {
    @Input()
    public app: AppDto;

    @Input()
    public usage: CallsUsageDto;

    @Input()
    public isStacked?: boolean | null;

    @Output()
    public isStackedChange = new EventEmitter<boolean>();

    public get chartOptions() {
        return this.isStacked ? ChartOptions.Stacked : ChartOptions.Default;
    }

    public chartData: any;
    public chartSummary = 0;

    public ngOnChanges() {
        if (this.usage) {
            const labels = ChartHelpers.createLabelsFromSet(this.usage.details);

            this.chartData = {
                labels,
                datasets: Object.keys(this.usage.details).map((k, i) => (
                    {
                        label: ChartHelpers.label(k),
                        backgroundColor: ChartHelpers.getBackgroundColor(i),
                        borderColor: ChartHelpers.getBorderColor(i),
                        borderWidth: 1,
                        data: this.usage.details[k].map(x => x.averageElapsedMs),
                    })),
            };

            this.chartSummary = this.usage.averageElapsedMs;
        }
    }
}
