/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { AssetFolderDto, AssetPathItem } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder',
    styleUrls: ['./asset-folder.component.scss'],
    templateUrl: './asset-folder.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetFolderComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Output()
    public delete = new EventEmitter<AssetPathItem>();

    @Input()
    public assetFolder: AssetFolderDto | AssetPathItem;

    public preventSelection(mouseEvent: MouseEvent) {
        if (mouseEvent.detail > 1) {
            mouseEvent.preventDefault();
        }
    }

    public emitNavigate() {
        this.navigate.emit(this.assetFolder);
    }

    public emitDelete() {
        this.delete.emit(this.assetFolder);
    }
}