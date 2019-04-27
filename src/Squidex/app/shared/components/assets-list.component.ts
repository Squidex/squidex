/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AssetDto,
    AssetsState,
    AssetWithUpload
} from '@app/shared/internal';

@Component({
    selector: 'sqx-assets-list',
    styleUrls: ['./assets-list.component.scss'],
    templateUrl: './assets-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsListComponent {
    @Input()
    public state: AssetsState;

    @Input()
    public isDisabled = false;

    @Input()
    public isListView = false;

    @Input()
    public selectedIds: object;

    @Output()
    public select = new EventEmitter<AssetDto>();

    public search() {
        this.state.load().pipe(onErrorResumeNext()).subscribe();
    }

    public delete(asset: AssetDto) {
        this.state.delete(asset).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.state.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.state.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public update(asset: AssetDto) {
        this.state.update(asset);
    }

    public updateFile(asset: AssetDto, file: File) {
        this.state.replaceFile(asset, file).pipe(onErrorResumeNext()).subscribe();
    }

    public emitSelect(asset: AssetDto) {
        this.select.emit(asset);
    }

    public isSelected(asset: AssetDto) {
        return this.selectedIds && this.selectedIds[asset.id];
    }

    public addFiles(files: File[]) {
        for (let file of files) {
            this.state.upload(file);
        }

        return true;
    }

    public trackByAsset(index: number, asset: AssetDto) {
        return asset.id;
    }

    public trackByUpload(index: number, upload: AssetWithUpload) {
        return upload.asset.id;
    }
}

