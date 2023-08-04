/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetFolderDto, AssetPathItem, DialogModel, ModalModel, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder',
    styleUrls: ['./asset-folder.component.scss'],
    templateUrl: './asset-folder.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetFolderComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Output()
    public delete = new EventEmitter<AssetFolderDto>();

    @Input()
    public isDisabled?: boolean | null;

    @Input({ required: true })
    public assetPathItem!: AssetPathItem;

    public editDropdown = new ModalModel();
    public editDialog = new DialogModel();

    public get assetFolder(): AssetFolderDto {
        return this.assetPathItem as any;
    }

    public get canUpdate() {
        return Types.is(this.assetPathItem, AssetFolderDto) && this.assetPathItem.canUpdate;
    }

    public get canDelete() {
        return Types.is(this.assetPathItem, AssetFolderDto) && this.assetPathItem.canDelete;
    }

    public preventSelection(mouseEvent: MouseEvent) {
        if (mouseEvent.detail > 1) {
            mouseEvent.preventDefault();
        }
    }

    public emitDelete() {
        if (this.isDisabled) {
            return;
        }

        this.delete.emit(this.assetFolder);
    }

    public emitNavigate() {
        if (this.isDisabled) {
            return;
        }

        this.navigate.emit(this.assetPathItem);
    }
}
