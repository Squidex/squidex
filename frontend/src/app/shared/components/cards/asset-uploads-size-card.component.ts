/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { ChartHelpers, ChartOptions, StorageUsagePerDateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploads-size-card[usage]',
    styleUrls: ['./asset-uploads-size-card.component.scss'],
    templateUrl: './asset-uploads-size-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetUploadsSizeCardComponent implements OnChanges {
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
                        data: this.usage.map(x => Math.round(100 * (x.totalSize / (1024 * 1024))) / 100),
                    },
                ],
            };
        }
    }
}
