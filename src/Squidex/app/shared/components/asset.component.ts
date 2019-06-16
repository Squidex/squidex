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
    fadeAnimation,
    StatefulComponent,
    Types,
    UploadCanceled
} from '@app/shared/internal';

interface State {
    progress: number;
}

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetComponent extends StatefulComponent<State> implements OnInit {
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
    public allTags: string[];

    @Output()
    public load = new EventEmitter<AssetDto>();

    @Output()
    public loadError = new EventEmitter();

    @Output()
    public remove = new EventEmitter<AssetDto>();

    @Output()
    public update = new EventEmitter<AssetDto>();

    @Output()
    public delete = new EventEmitter<AssetDto>();

    @Output()
    public select = new EventEmitter<AssetDto>();

    public editDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly assetUploader: AssetUploaderState,
        private readonly dialogs: DialogService
    ) {
        super(changeDetector, {
            progress: 0
        });
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
        this.update.emit(this.asset);
    }

    public emitRemove() {
        this.remove.emit(this.asset);
    }

    private setProgress(progress: number) {
        this.next(s => ({ ...s, progress }));
    }

    public updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;

        if (emitEvent) {
            this.emitUpdate();
        }

        this.next(s => ({ ...s, progress: 0 }));

        this.cancelEdit();
    }
}