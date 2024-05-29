/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FileSizePipe, TranslatePipe } from '@app/framework';
import { CurrentStorageDto } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-asset-uploads-size-summary-card',
    styleUrls: ['./asset-uploads-size-summary-card.component.scss'],
    templateUrl: './asset-uploads-size-summary-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FileSizePipe,
        TranslatePipe,
    ],
})
export class AssetUploadsSizeSummaryCardComponent {
    @Input({ required: true })
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
