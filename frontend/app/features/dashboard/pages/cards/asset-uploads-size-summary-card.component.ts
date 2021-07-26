/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { AppDto, CurrentStorageDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-asset-uploads-size-summary-card[app][usage]',
    styleUrls: ['./asset-uploads-size-summary-card.component.scss'],
    templateUrl: './asset-uploads-size-summary-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetUploadsSizeSummaryCardComponent implements OnChanges {
    @Input()
    public app: AppDto;

    @Input()
    public usage: CurrentStorageDto;

    public storageCurrent = 0;
    public storageAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.storageCurrent = this.usage.size;
            this.storageAllowed = this.usage.maxAllowed;
        }
    }
}
