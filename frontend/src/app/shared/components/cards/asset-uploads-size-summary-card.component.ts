/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { FileSizePipe, TranslatePipe } from '@app/framework';
import { CurrentStorageDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploads-size-summary-card',
    styleUrls: ['./asset-uploads-size-summary-card.component.scss'],
    templateUrl: './asset-uploads-size-summary-card.component.html',
    imports: [
        FileSizePipe,
        TranslatePipe,
    ],
})
export class AssetUploadsSizeSummaryCardComponent {
    /** The asset storage usage of the app. */
    public readonly usage = input.required<CurrentStorageDto | undefined>();

    protected readonly storageCurrent = computed(() => {
        return this.usage()?.size ?? 0;
    });

    protected readonly storageAllowed = computed(() => {
        return this.usage()?.maxAllowed ?? 0;
    });
}