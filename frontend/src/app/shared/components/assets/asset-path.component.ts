/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetPathItem } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-path',
    styleUrls: ['./asset-path.component.scss'],
    templateUrl: './asset-path.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetPathComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Input()
    public path?: ReadonlyArray<AssetPathItem> | null;

    @Input({ transform: booleanAttribute })
    public all?: boolean | null;
}
