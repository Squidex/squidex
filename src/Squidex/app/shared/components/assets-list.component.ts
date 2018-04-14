/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:prefer-for-of

import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
    AssetsState,
    AssetDto,
    ImmutableArray
} from '@app/shared/internal';

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html'
})
export class AssetsListComponent {
    public newFiles = ImmutableArray.empty<File>();

    @Input()
    public state: AssetsState;

    @Input()
    public isDisabled: false;

    @Input()
    public selectedIds: object;

    @Input()
    public assetClass = '';

    @Output()
    public selected = new EventEmitter<AssetDto>();

    public onAssetLoaded(file: File, asset: AssetDto) {
        this.newFiles = this.newFiles.remove(file);

        this.state.addAsset(asset);
    }

    public search() {
        this.state.loadAssets().subscribe();
    }

    public onAssetDeleting(asset: AssetDto) {
        this.state.delete(asset).subscribe();
    }

    public onAssetSelected(asset: AssetDto) {
        this.selected.emit(asset);
    }

    public onAssetFailed(file: File) {
        this.newFiles = this.newFiles.remove(file);
    }

    public goNext() {
        this.state.goNext().subscribe();
    }

    public goPrev() {
        this.state.goPrev().subscribe();
    }

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }

    public isSelected(asset: AssetDto) {
        return this.selectedIds && this.selectedIds[asset.id];
    }

    public addFiles(files: FileList) {
        for (let i = 0; i < files.length; i++) {
            this.newFiles = this.newFiles.pushFront(files[i]);
        }
    }
}

