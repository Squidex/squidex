/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ChartData } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { TranslatePipe } from '@app/framework';
import { ChartHelpers, ChartOptions, StorageUsagePerDateDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploads-size-card',
    styleUrls: ['./asset-uploads-size-card.component.scss'],
    templateUrl: './asset-uploads-size-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        BaseChartDirective,
        TranslatePipe,
    ],
})
export class AssetUploadsSizeCardComponent {
    /** The asset storage usage per day. */
    public readonly usage = input.required<ReadonlyArray<StorageUsagePerDateDto> | undefined>();

    public readonly chartOptions = ChartOptions.Default;

    public readonly chartData = computed<ChartData | undefined>(() => {
        const usage = this.usage();
        if (!usage) {
            return undefined;
        }

        return {
            labels: [...ChartHelpers.createLabels(usage)],
            datasets: [
                {
                    label: 'All',
                    lineTension: 0,
                    fill: false,
                    backgroundColor: ChartHelpers.getBackgroundColor(),
                    borderColor: ChartHelpers.getBorderColor(),
                    borderWidth: 1,
                    data: usage.map(x => Math.round(100 * (x.totalSize / (1024 * 1024))) / 100),
                },
            ],
        };
    });
}
