/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';

import {
    AssetDto,
    AssetsState,
    AssetUploaderState,
    AuthService,
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
    public delete = new EventEmitter();

    @Output()
    public select = new EventEmitter();

    @Input()
    public assetFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public assetsState: AssetsState;

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
        public readonly authService: AuthService,
        private readonly assetUploader: AssetUploaderState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnInit() {
        const assetFile = this.assetFile;

        if (assetFile) {
            this.setProgress(1);

            this.assetUploader.uploadFile(assetFile, this.assetsState)
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
                .subscribe(asset => {
                    if (Types.isNumber(asset)) {
                        this.setProgress(asset);
                    } else {
                        this.setProgress(0);

                        this.asset = asset;
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

    public emitRemove() {
        this.remove.emit();
    }

    private setProgress(progress: number) {
        this.progress = progress;

        this.changeDetector.markForCheck();
    }
}