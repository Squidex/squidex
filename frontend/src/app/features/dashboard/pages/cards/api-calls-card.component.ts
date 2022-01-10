/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { AppDto, CallsUsageDto, UsagesService } from '@app/shared';
import { ChartHelpers, ChartOptions } from './shared';

@Component({
    selector: 'sqx-api-calls-card[app][usage]',
    styleUrls: ['./api-calls-card.component.scss'],
    templateUrl: './api-calls-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiCallsCardComponent implements OnChanges {
    @Input()
    public app!: AppDto;

    @Input()
    public usage?: CallsUsageDto;

    public chartOptions = ChartOptions.Stacked;
    public chartData: any;

    constructor(
        private readonly usagesService: UsagesService,
    ) {
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
                        data: value.map(x => x.totalCalls),
                    })),
            };
        }
    }

    public downloadLog() {
        this.usagesService.getLog(this.app.name)
            .subscribe(url => {
                window.open(url, '_blank');
            });
    }
}
