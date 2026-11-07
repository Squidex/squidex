/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalPlacementDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetFolderDto, AssetPathItem, DialogModel, ModalModel, Types } from '@app/shared/internal';
import { AssetFolderDialogComponent } from './asset-folder-dialog.component';

@Component({
    selector: 'sqx-asset-folder',
    styleUrls: ['./asset-folder.component.scss'],
    templateUrl: './asset-folder.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetFolderDialogComponent,
        ConfirmClickDirective,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetFolderComponent {
    /** Emits the folder when the user opens it. */
    public readonly navigate = output<AssetPathItem>();

    /** Emits the folder when the user deletes it. */
    public readonly delete = output<AssetFolderDto>();

    /** Prevents opening, renaming and deleting the folder. */
    public readonly isDisabled = input(false, { transform: booleanAttribute });

    /** The folder to render. */
    public readonly assetPathItem = input.required<AssetPathItem>();

    public readonly editDropdown = new ModalModel();
    public readonly editDialog = new DialogModel();

    public readonly assetFolder = computed(() => {
        return this.assetPathItem() as AssetFolderDto;
    });

    public readonly canUpdate = computed(() => {
        const item = this.assetPathItem();

        return Types.is(item, AssetFolderDto) && item.canUpdate;
    });

    public readonly canDelete = computed(() => {
        const item = this.assetPathItem();

        return Types.is(item, AssetFolderDto) && item.canDelete;
    });

    public preventSelection(mouseEvent: MouseEvent) {
        if (mouseEvent.detail > 1) {
            mouseEvent.preventDefault();
        }
    }

    public emitDelete() {
        if (this.isDisabled()) {
            return;
        }

        this.delete.emit(this.assetFolder());
    }

    public emitNavigate() {
        if (this.isDisabled()) {
            return;
        }

        this.navigate.emit(this.assetPathItem());
    }
}
