/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { ChartHelpers, ChartOptions, StorageUsagePerDateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploads-count-card[usage]',
    styleUrls: ['./asset-uploads-count-card.component.scss'],
    templateUrl: './asset-uploads-count-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetUploadsCountCardComponent implements OnChanges {
    @Input()
    public usage?: ReadonlyArray<StorageUsagePerDateDto>;

    public chartData: any;
    public chartOptions = ChartOptions.Default;

    public ngOnChanges() {
        if (this.usage) {
            const labels = ChartHelpers.createLabels(this.usage);

            this.chartData = {
                labels,
                datasets: [
                    {
                        label: 'All',
                        lineTension: 0,
                        fill: false,
                        backgroundColor: ChartHelpers.getBackgroundColor(),
                        borderColor: ChartHelpers.getBorderColor(),
                        borderWidth: 1,
                        data: this.usage.map(x => x.totalCount),
                    },
                ],
            };
        }
    }
}
