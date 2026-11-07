/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';
import { FileDropDirective, HTTP, TourStepDirective, TranslatePipe } from '@app/framework';
import { AssetDto, AssetFolderDto, AssetsState, getFiles, Types } from '@app/shared/internal';
import { AssetFolderComponent } from './asset-folder.component';
import { AssetComponent } from './asset.component';

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetComponent,
        AssetFolderComponent,
        AsyncPipe,
        CdkDrag,
        CdkDropList,
        CdkDropListGroup,
        FileDropDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AssetsListComponent {
    /** Emits the asset that should be edited. */
    public readonly edit = output<AssetDto>();

    /** Emits the asset that has been clicked. */
    public readonly assetSelect = output<AssetDto>();

    /** The state with the assets and folders to show. */
    public readonly assetsState = input.required<AssetsState>();

    /** Disables uploading, editing, deleting and moving of assets. */
    public readonly isDisabled = input(false, { transform: booleanAttribute });

    /** Renders the assets as rows instead of cards. */
    public readonly isListView = input(false, { transform: booleanAttribute });

    /** The selected assets by ID. Makes the assets selectable when defined. */
    public readonly selectedIds = input<Record<string, AssetDto>>();

    /** Shows a button to open the folder of an asset, when no folder is selected. */
    public readonly showFolderIcon = input(true, { transform: booleanAttribute });

    public readonly newFiles = signal<ReadonlyArray<HTTP.UploadFile>>([]);

    public add(file: HTTP.UploadFile, asset: AssetDto) {
        if (asset.isDuplicate) {
            setTimeout(() => {
                this.remove(file);
            }, 2000);
        } else {
            this.remove(file);

            this.assetsState().addAsset(asset);
        }
    }

    public move(drag: CdkDragDrop<any>) {
        if (this.isDisabled() || !drag.isPointerOverContainer) {
            return;
        }

        const item = drag.item.data;
        if (Types.is(item, AssetDto)) {
            this.assetsState().moveAsset(item, drag.container.data);
        } else {
            this.assetsState().moveAssetFolder(item, drag.container.data);
        }
    }

    public replaceAsset(asset: AssetDto) {
        this.assetsState().replaceAsset(asset);
    }

    public selectFolder(asset: AssetDto) {
        this.assetsState().navigate(asset.parentId);
    }

    public deleteAsset(asset: AssetDto) {
        this.assetsState().deleteAsset(asset);
    }

    public deleteAssetFolder(assetFolder: AssetFolderDto) {
        this.assetsState().deleteAssetFolder(assetFolder);
    }

    public isSelected(asset: AssetDto) {
        return !!this.selectedIds()?.[asset.id];
    }

    public remove(file: HTTP.UploadFile) {
        this.newFiles.update(s => s.removed(file));

        return true;
    }

    public addFiles(files: ReadonlyArray<HTTP.UploadFile>) {
        this.newFiles.update(s => [...getFiles(files), ...s]);

        return true;
    }

    public canEnter(drag: CdkDrag, drop: CdkDropList) {
        return drag.data.id !== drop.data;
    }
}
