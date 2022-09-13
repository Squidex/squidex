/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { CurrentStorageDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploads-size-summary-card[usage]',
    styleUrls: ['./asset-uploads-size-summary-card.component.scss'],
    templateUrl: './asset-uploads-size-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetUploadsSizeSummaryCardComponent implements OnChanges {
    @Input()
    public usage?: CurrentStorageDto;

    public storageCurrent = 0;
    public storageAllowed = 0;

    public ngOnChanges() {
        if (this.usage) {
            this.storageCurrent = this.usage.size;
            this.storageAllowed = this.usage.maxAllowed;
        }
    }
}
