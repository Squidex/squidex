/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetFolderDto, AssetPathItem, DialogModel, fadeAnimation, ModalModel, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder',
    styleUrls: ['./asset-folder.component.scss'],
    templateUrl: './asset-folder.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetFolderComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Output()
    public delete = new EventEmitter<AssetPathItem>();

    @Input()
    public assetFolder: AssetFolderDto | AssetPathItem;

    public dropdown = new ModalModel();

    public editDialog = new DialogModel();

    public get canUpdate() {
        return Types.is(this.assetFolder, AssetFolderDto) && this.assetFolder.canUpdate;
    }

    public get canDelete() {
        return Types.is(this.assetFolder, AssetFolderDto) && this.assetFolder.canDelete;
    }

    public preventSelection(mouseEvent: MouseEvent) {
        if (mouseEvent.detail > 1) {
            mouseEvent.preventDefault();
        }
    }

    public emitDelete() {
        this.delete.emit(this.assetFolder);
    }

    public emitNavigate() {
        this.navigate.emit(this.assetFolder);
    }
}