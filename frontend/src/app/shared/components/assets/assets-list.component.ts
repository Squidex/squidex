/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDropList, CdkDropListGroup } from '@angular/cdk/drag-drop';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { FileDropDirective } from '@app/framework';
import { TourStepDirective } from '@app/framework';
import { AssetDto, AssetFolderDto, AssetsState, getFiles, StatefulComponent, Types } from '@app/shared/internal';
import { AssetFolderComponent } from './asset-folder.component';
import { AssetComponent } from './asset.component';

interface State {
    // The new files.
    newFiles: ReadonlyArray<File>;
}

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        TourStepDirective,
        FileDropDirective,
        CdkDropListGroup,
        CdkDropList,
        AssetFolderComponent,
        NgFor,
        CdkDrag,
        AssetComponent,
        AsyncPipe,
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
