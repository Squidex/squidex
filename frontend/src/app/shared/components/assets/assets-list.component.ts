/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';
import { AssetDto, AssetFolderDto, AssetsState, getFiles, StatefulComponent, Types } from '@app/shared/internal';

interface State {
    // The new files.
    newFiles: ReadonlyArray<File>;
}

@Component({
    selector: 'sqx-assets-list[assetsState]',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetsListComponent extends StatefulComponent<State> {
    @Output()
    public select = new EventEmitter<AssetDto>();

    @Input()
    public assetsState!: AssetsState;

    @Input()
    public isDisabled?: boolean | null;

    @Input()
    public isListView?: boolean | null;

    @Input()
    public indicateLoading?: boolean | null;

    @Input()
    public selectedIds?: {};

    @Input()
    public showPager?: boolean | null = true;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            newFiles: [],
        });
    }

    public add(file: File, asset: AssetDto) {
        if (asset.isDuplicate) {
            setTimeout(() => {
                this.remove(file);
            }, 2000);
        } else {
            this.remove(file);

            this.assetsState.addAsset(asset);
        }
    }

    public move(drag: CdkDragDrop<any>) {
        if (!this.isDisabled && drag.isPointerOverContainer) {
            const item = drag.item.data;

            if (Types.is(item, AssetDto)) {
                this.assetsState.moveAsset(item, drag.container.data);
            } else {
                this.assetsState.moveAssetFolder(item, drag.container.data);
            }
        }
    }

    public reloadTotal() {
        this.assetsState.load(true, false);
    }

    public selectFolder(asset: AssetDto) {
        this.assetsState.navigate(asset.parentId);
    }

    public deleteAsset(asset: AssetDto) {
        this.assetsState.deleteAsset(asset);
    }

    public deleteAssetFolder(assetFolder: AssetFolderDto) {
        this.assetsState.deleteAssetFolder(assetFolder);
    }

    public isSelected(asset: AssetDto) {
        return this.selectedIds && this.selectedIds[asset.id];
    }

    public remove(file: File) {
        this.next(s => ({
            ...s,
            newFiles: s.newFiles.removed(file),
        }));

        return true;
    }

    public addFiles(files: ReadonlyArray<File>) {
        this.next(s => ({
            ...s,
            newFiles: [...getFiles(files), ...s.newFiles],
        }));

        return true;
    }

    public canEnter(drag: CdkDrag, drop: CdkDropList) {
        return drag.data.id !== drop.data;
    }

    public trackByAssetFolder(_index: number, asset: AssetFolderDto) {
        return asset.id;
    }

    public trackByAsset(_index: number, asset: AssetDto) {
        return asset.id;
    }
}
