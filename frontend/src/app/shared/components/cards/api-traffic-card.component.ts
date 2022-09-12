/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CallsUsageDto, ChartHelpers, ChartOptions } from '@app/shared/internal';

@Component({
    selector: 'sqx-api-traffic-card[usage]',
    styleUrls: ['./api-traffic-card.component.scss'],
    templateUrl: './api-traffic-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiTrafficCardComponent implements OnChanges {
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

    public ngOnChanges(changes: SimpleChanges) {
        if (this.usage && changes['usage']) {
            const labels = ChartHelpers.createLabelsFromSet(this.usage.details);

            this.chartData = {
                labels,
                datasets: Object.entries(this.usage.details).map(([key, value], i) => (
                    {
                        label: ChartHelpers.label(key),
                        backgroundColor: ChartHelpers.getBackgroundColor(i),
                        borderColor: ChartHelpers.getBorderColor(i),
                        borderWidth: 1,
                        data: value.map(x => Math.round(100 * (x.totalBytes / (1024 * 1024))) / 100),
                    })),
            };

            this.chartSummary = this.usage.totalBytes;
        }
    }
}
