/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    AssetDto,
    AssetsState,
    getFiles
} from '@app/shared/internal';

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
    public selectedIds: object;

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

            this.state.add(asset);
        }
    }

    public search() {
        this.state.load();
    }

    public delete(asset: AssetDto) {
        this.state.delete(asset);
    }

    public update(asset: AssetDto) {
        this.state.update(asset);
    }

    public emitSelect(asset: AssetDto) {
        this.select.emit(asset);
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

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }
}
