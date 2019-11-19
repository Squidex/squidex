/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';

import {
    AssetDto,
    AssetUploaderState,
    DialogModel,
    DialogService,
    Types,
    UploadCanceled
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetComponent implements OnInit {
    @Output()
    public load = new EventEmitter<AssetDto>();

    @Output()
    public loadError = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Output()
    public update = new EventEmitter();

    @Output()
    public delete = new EventEmitter();

    @Output()
    public select = new EventEmitter();

    @Input()
    public initFile: File;

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
    public allTags: ReadonlyArray<string>;

    public progress = 0;

    public editDialog = new DialogModel();

    constructor(
        private readonly assetUploader: AssetUploaderState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.setProgress(1);

            this.assetUploader.uploadFile(initFile)
                .subscribe(dto => {
                    if (Types.isNumber(dto)) {
                        this.setProgress(dto);
                    } else {
                        this.emitLoad(dto);
                    }
                }, error => {
                    if (!Types.is(error, UploadCanceled)) {
                        this.dialogs.notifyError(error);
                    }

                    this.emitLoadError(error);
                });
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1 && this.asset.canUpload) {
            this.setProgress(1);

            this.assetUploader.uploadAsset(this.asset, files[0])
                .subscribe(dto => {
                    if (Types.isNumber(dto)) {
                        this.setProgress(dto);
                    } else {
                        this.updateAsset(dto, true);
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.setProgress(0);
                }, () => {
                    this.setProgress(0);
                });
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

    public emitLoad(asset: AssetDto) {
        this.load.emit(asset);
    }

    public emitLoadError(error: any) {
        this.loadError.emit(error);
    }

    public emitUpdate() {
        this.update.emit();
    }

    public emitRemove() {
        this.remove.emit();
    }

    private setProgress(progress: number) {
        this.progress = progress;

        this.changeDetector.markForCheck();
    }

    public updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;

        if (emitEvent) {
            this.emitUpdate();
        }

        this.setProgress(0);

        this.cancelEdit();
    }
}