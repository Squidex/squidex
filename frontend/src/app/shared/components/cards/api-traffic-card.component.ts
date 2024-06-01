/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { FileSizePipe, TranslatePipe } from '@app/framework';
import { CallsUsageDto, ChartHelpers, ChartOptions, TypedSimpleChanges } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-api-traffic-card',
    styleUrls: ['./api-traffic-card.component.scss'],
    templateUrl: './api-traffic-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        BaseChartDirective,
        FileSizePipe,
        FormsModule,
        TranslatePipe,
    ],
})
export class ApiTrafficCardComponent {
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (this.usage && changes.usage) {
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
