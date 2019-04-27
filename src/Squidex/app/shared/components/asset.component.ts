/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, HostBinding, Input, Output } from '@angular/core';

import {
    AssetDto,
    DialogModel,
    fadeAnimation,
    UploadingAsset
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetComponent {
    @Input()
    public upload: UploadingAsset;

    @Input()
    public asset: AssetDto;

    @Input()
    public removeMode = false;

    @Input()
    public isCompact = false;

    @Input()
    public isDisabled = false;

    @Input()
    public isSelected = false;

    @Input()
    public isSelectable = false;

    @Input() @HostBinding('class.isListView')
    public isListView = false;

    @Input()
    public allTags: string[];

    @Output()
    public remove = new EventEmitter<AssetDto>();

    @Output()
    public update = new EventEmitter<AssetDto>();

    @Output()
    public uploadFile = new EventEmitter<File>();

    @Output()
    public delete = new EventEmitter<AssetDto>();

    @Output()
    public select = new EventEmitter<AssetDto>();

    public editDialog = new DialogModel();

    public updateFile(files: File[]) {
        if (files.length === 1) {
            this.uploadFile.emit(files[0]);
        }
    }

    public edit() {
        if (!this.isDisabled) {
            this.editDialog.show();
        }
    }

    public cancelEdit() {
        this.editDialog.hide();
    }

    public emitSelect() {
        this.select.emit(this.asset);
    }

    public emitDelete() {
        this.delete.emit(this.asset);
    }

    public emitUpdate() {
        this.update.emit(this.asset);
    }

    public emitRemove() {
        this.remove.emit(this.asset);
    }

    public updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;

        if (emitEvent) {
            this.emitUpdate();
        }

        this.cancelEdit();
    }
}