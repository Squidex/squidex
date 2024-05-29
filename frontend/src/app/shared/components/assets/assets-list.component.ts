/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FileDropDirective, HTTP, TourStepDirective, TranslatePipe } from '@app/framework';
import { AssetDto, AssetFolderDto, AssetsState, getFiles, StatefulComponent, Types } from '@app/shared/internal';
import { AssetFolderComponent } from './asset-folder.component';
import { AssetComponent } from './asset.component';

interface State {
    // The new files.
    newFiles: ReadonlyArray<HTTP.UploadFile>;
}

@Component({
    standalone: true,
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
export class AssetsListComponent extends StatefulComponent<State> {
    @Output()
    public edit = new EventEmitter<AssetDto>();

    @Output()
    public assetSelect = new EventEmitter<AssetDto>();

    @Input({ required: true })
    public assetsState!: AssetsState;

    @Input({ transform: booleanAttribute })
    public isDisabled?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isListView?: boolean | null;

    @Input()
    public selectedIds?: Record<string, AssetDto>;

    @Input({ transform: booleanAttribute })
    public showFolderIcon?: boolean | null = true;

    constructor() {
        super({ newFiles: [] });
    }

    public add(file: HTTP.UploadFile, asset: AssetDto) {
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

    public replaceAsset(asset: AssetDto) {
        this.assetsState.replaceAsset(asset);
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

    public remove(file: HTTP.UploadFile) {
        this.next(s => ({
            ...s,
            newFiles: s.newFiles.removed(file),
        }));

        return true;
    }

    public addFiles(files: ReadonlyArray<HTTP.UploadFile>) {
        this.next(s => ({
            ...s,
            newFiles: [...getFiles(files), ...s.newFiles],
        }));

        return true;
    }

    public canEnter(drag: CdkDrag, drop: CdkDropList) {
        return drag.data.id !== drop.data;
    }
}
