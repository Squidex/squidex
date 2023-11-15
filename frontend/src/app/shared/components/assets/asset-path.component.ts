/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { AssetPathItem } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-asset-path',
    styleUrls: ['./asset-path.component.scss'],
    templateUrl: './asset-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgFor,
        NgIf,
        TranslatePipe,
    ],
})
export class AssetPathComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Input()
    public path?: ReadonlyArray<AssetPathItem> | null;

    @Input({ transform: booleanAttribute })
    public all?: boolean | null;
}
