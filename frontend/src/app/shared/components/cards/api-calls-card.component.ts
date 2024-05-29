/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { TranslatePipe } from '@app/framework';
import { AppDto, CallsUsageDto, ChartHelpers, ChartOptions, UsagesService } from '@app/shared/internal';

@Component({
    standalone: true,
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
    @Input()
    public app: AppDto | undefined | null;

    @Input({ required: true })
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

    public downloadLog(app: AppDto) {
        this.usagesService.getLog(app.name)
            .subscribe(url => {
                window.open(url, '_blank');
            });
    }
}
