/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, HostBinding, Input, OnInit, Output } from '@angular/core';
import { ConfirmClickDirective, ExternalLinkDirective, FileDropDirective, FromNowPipe, HTTP, ImageSourceDirective, ProgressBarComponent, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetDto, AssetUploaderState, DialogService, StatefulComponent, Types, UploadCanceled } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { AssetPreviewUrlPipe, AssetUrlPipe, FileIconPipe } from './pipes';

interface State {
    // The download progress.
    progress: number;
}

@Component({
    standalone: true,
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        ConfirmClickDirective,
        ExternalLinkDirective,
        FileDropDirective,
        FileIconPipe,
        FromNowPipe,
        ImageSourceDirective,
        ProgressBarComponent,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class AssetComponent extends StatefulComponent<State> implements OnInit {
    @Output()
    public loadDone = new EventEmitter<AssetDto>();

    @Output()
    public loadError = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Output()
    public delete = new EventEmitter();

    @Output()
    public edit = new EventEmitter<AssetDto>();

    @Output()
    public selectAsset = new EventEmitter();

    @Output()
    public selectFolder = new EventEmitter<string>();

    @Input()
    public assetFile?: HTTP.UploadFile;

    @Input()
    public asset?: AssetDto;

    @Input()
    public folderId?: string;

    @Input({ transform: booleanAttribute })
    public folderIcon?: boolean | null | undefined;

    @Input({ transform: booleanAttribute })
    public removeMode?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isDisabled?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isSelected?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isCompact: boolean | undefined | null;

    @Input({ transform: booleanAttribute })
    public isSelectable?: boolean | null;

    @Input({ transform: booleanAttribute }) @HostBinding('class.isListView')
    public isListView?: boolean | null;

    constructor(
        private readonly assetUploader: AssetUploaderState,
        private readonly dialogs: DialogService,
    ) {
        super({ progress: 0 });
    }

    public ngOnInit() {
        const assetFile = this.assetFile;

        if (assetFile) {
            this.setProgress(1);

            this.assetUploader.uploadFile(assetFile, this.folderId)
                .subscribe({
                    next: assetOrProgress => {
                        if (Types.isNumber(assetOrProgress)) {
                            this.setProgress(assetOrProgress);
                        } else {
                            this.emitLoad(assetOrProgress);
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

    public updateFile(files: ReadonlyArray<HTTP.UploadFile>) {
        const asset = this.asset;

        if (files.length === 1 && asset?.canUpload) {
            this.dialogs.confirm('i18n:assets.replaceConfirmTitle', 'i18n:assets.replaceConfirmText')
                .subscribe(confirmed => {
                    if (confirmed) {
                        this.setProgress(1);

                        this.assetUploader.uploadAsset(asset, files[0])
                            .subscribe({
                                next: assetOrProgress => {
                                    if (Types.isNumber(assetOrProgress)) {
                                        this.setProgress(assetOrProgress);
                                    } else {
                                        this.emitLoad(assetOrProgress);
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

    public emitEdit() {
        if (!this.isDisabled) {
            this.edit.emit(this.asset);
        }
    }

    public emitLoad(asset: AssetDto) {
        this.loadDone.emit(asset);
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
