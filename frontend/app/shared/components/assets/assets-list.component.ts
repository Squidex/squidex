/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetDto, AssetFolderDto, AssetsState, getFiles, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsListComponent {
    @Output()
    public select = new EventEmitter<AssetDto>();

    @Input()
    public state: AssetsState;

    @Input()
    public isDisabled = false;

    @Input()
    public isListView = false;

    @Input()
    public indicateLoading = false;

    @Input()
    public selectedIds: object;

    @Input()
    public showPager = true;

    public newFiles: ReadonlyArray<File> = [];

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public add(file: File, asset: AssetDto) {
        if (asset.isDuplicate) {
            setTimeout(() => {
                this.newFiles = this.newFiles.removed(file);

                this.changeDetector.markForCheck();
            }, 2000);
        } else {
            this.newFiles = this.newFiles.removed(file);

            this.state.addAsset(asset);
        }
    }

    public move(drag: CdkDragDrop<any>) {
        if (!this.isDisabled && drag.isPointerOverContainer) {
            const item = drag.item.data;

            if (Types.is(item, AssetDto)) {
                this.state.moveAsset(item, drag.container.data);
            } else {
                this.state.moveAssetFolder(item, drag.container.data);
            }
        }
    }

    public selectFolder(asset: AssetDto) {
        this.state.navigate(asset.parentId);
    }

    public deleteAsset(asset: AssetDto) {
        this.state.deleteAsset(asset);
    }

    public deleteAssetFolder(assetFolder: AssetFolderDto) {
        this.state.deleteAssetFolder(assetFolder);
    }

    public isSelected(asset: AssetDto) {
        return this.selectedIds && this.selectedIds[asset.id];
    }

    public remove(file: File) {
        this.newFiles = this.newFiles.removed(file);
    }

    public addFiles(files: ReadonlyArray<File>) {
        this.newFiles = [...getFiles(files), ...this.newFiles];

        return true;
    }

    public canEnter(drag: CdkDrag, drop: CdkDropList) {
        return drag.data.id !== drop.data;
    }

    public trackByAssetItem(_index: number, asset: AssetDto | AssetFolderDto) {
        return asset.id;
    }
}
