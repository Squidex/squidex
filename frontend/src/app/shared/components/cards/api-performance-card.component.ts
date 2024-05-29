/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { TranslatePipe } from '@app/framework';
import { CallsUsageDto, ChartHelpers, ChartOptions } from '@app/shared';

@Component({
    standalone: true,
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
    @Input({ required: true })
    public usage?: CallsUsageDto;

    @Input({ transform: booleanAttribute })
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
