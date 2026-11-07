/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { AssetPathItem } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-path',
    styleUrls: ['./asset-path.component.scss'],
    templateUrl: './asset-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class AssetPathComponent {
    /** Emits the folder to navigate to. */
    public readonly navigate = output<AssetPathItem>();

    /** The folders from the root to the current folder. */
    public readonly path = input<ReadonlyArray<AssetPathItem> | null>();

    /** Highlights all folders, including the current folder. */
    public readonly all = input(false, { transform: booleanAttribute });
}
