/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';
import { AssetDto, AssetsState, AssetUploaderState, DialogModel, DialogService, StatefulComponent, Types, UploadCanceled } from '@app/shared/internal';

interface State {
    // The download progress.
    progress: number;
}

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetComponent extends StatefulComponent<State> implements OnInit {
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

    @Output()
    public selectFolder = new EventEmitter<string>();

    @Input()
    public assetFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public assetsState: AssetsState;

    @Input()
    public folderId?: string;

    @Input()
    public removeMode?: boolean | null;

    @Input()
    public isDisabled?: boolean | null;

    @Input()
    public isSelected?: boolean | null;

    @Input()
    public isCompact: boolean | undefined | null;

    @Input()
    public isSelectable?: boolean | null;

    @Input() @HostBinding('class.isListView')
    public isListView?: boolean | null;

    @Input()
    public allTags: ReadonlyArray<string>;

    public editDialog = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly assetUploader: AssetUploaderState,
        private readonly dialogs: DialogService,
    ) {
        super(changeDetector, {
            progress: 0,
        });
    }

    public ngOnInit() {
        const assetFile = this.assetFile;

        if (assetFile) {
            this.setProgress(1);

            this.assetUploader.uploadFile(assetFile, this.assetsState, this.folderId)
                .subscribe({
                    next: dto => {
                        if (Types.isNumber(dto)) {
                            this.setProgress(dto);
                        } else {
                            this.emitLoad(dto);
                        }
                    },
                    error: error => {
                        if (!Types.is(error, UploadCanceled)) {
                            this.dialogs.notifyError(error);
                        }

                        this.emitLoadError(error);
                    },
                });
        }
    }

    public updateFile(files: ReadonlyArray<File>) {
        if (files.length === 1 && this.asset.canUpload) {
            this.dialogs.confirm('i18n:assets.replaceConfirmTitle', 'i18n:assets.replaceConfirmText')
                .subscribe(confirmed => {
                    if (confirmed) {
                        this.setProgress(1);

                        this.assetUploader.uploadAsset(this.asset, files[0])
                            .subscribe({
                                next: asset => {
                                    if (Types.isNumber(asset)) {
                                        this.setProgress(asset);
                                    } else {
                                        this.setProgress(0);
                                        this.setAsset(asset);
                                    }
                                },
                                error: error => {
                                    this.dialogs.notifyError(error);

                                    this.setProgress(0);
                                },
                                complete: () => {
                                    this.setProgress(0);
                                },
                            });
                    }
                });
        }
    }

    public edit() {
        if (!this.isDisabled) {
            this.editDialog.show();
        }
    }

    public emitLoad(asset: AssetDto) {
        this.load.emit(asset);
    }

    public emitLoadError(error: any) {
        this.loadError.emit(error);
    }

    public setAsset(asset: AssetDto) {
        this.asset = asset;

        this.detectChanges();
    }

    public setProgress(progress: number) {
        this.next({ progress });
    }
}
