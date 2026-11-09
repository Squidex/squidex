/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, effect, inject, input, output, signal, untracked } from '@angular/core';
import { ConfirmClickDirective, ExternalLinkDirective, FileDropDirective, FromNowPipe, HTTP, ImageSourceDirective, ProgressBarComponent, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetDto, AssetUploaderState, DialogService, Types, UploadCanceled } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { AssetPreviewUrlPipe, AssetUrlPipe, FileIconPipe } from './pipes';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    host: {
        '[class.isListView]': 'isListView()',
    },
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
export class AssetComponent {
    private readonly assetUploader = inject(AssetUploaderState);
    private readonly dialogs = inject(DialogService);

    /** Emits the asset when the file has been uploaded or replaced. */
    public readonly loadDone = output<AssetDto>();

    /** Emits the error when the upload of the file has failed. */
    public readonly loadError = output<any>();

    /** Emits when the asset should be removed from the list (remove mode). */
    public readonly remove = output();

    /** Emits when the asset should be deleted. */
    public readonly delete = output();

    /** Emits when the asset should be edited. */
    public readonly edit = output<AssetDto>();

    /** Emits when the asset is clicked. */
    public readonly selectAsset = output();

    /** Emits the ID of the parent folder when the folder button is clicked. */
    public readonly selectFolder = output<string>();

    /** The file to upload as new asset. */
    public readonly assetFile = input<HTTP.UploadFile>();

    /** The asset to show. */
    public readonly asset = input<AssetDto>();

    /** The ID of the folder to upload the file to. */
    public readonly folderId = input<string>();

    /** Shows a button to open the folder of the asset. */
    public readonly folderIcon = input(false, { transform: booleanAttribute });

    /** Shows a button to remove the asset instead of deleting it. */
    public readonly removeMode = input(false, { transform: booleanAttribute });

    /** Hides the buttons to edit, delete or remove the asset. */
    public readonly isDisabled = input(false, { transform: booleanAttribute });

    /** Highlights the asset as selected. */
    public readonly isSelected = input(false, { transform: booleanAttribute });

    /** Hides the details in the list view. */
    public readonly isCompact = input(false, { transform: booleanAttribute });

    /** Indicates that the asset can be selected. */
    public readonly isSelectable = input(false, { transform: booleanAttribute });

    /** Renders the asset as row instead of a card. */
    public readonly isListView = input(false, { transform: booleanAttribute });

    protected readonly progress = signal(0);

    constructor() {
        effect(() => {
            const assetFile = this.assetFile();
            if (!assetFile) {
                return;
            }

            untracked(() => this.uploadFile(assetFile));
        });
    }

    private uploadFile(assetFile: HTTP.UploadFile) {
        this.progress.set(1);

        this.assetUploader.uploadFile(assetFile, this.folderId())
            .subscribe({
                next: assetOrProgress => {
                    if (Types.isNumber(assetOrProgress)) {
                        this.progress.set(assetOrProgress);
                    } else {
                        this.loadDone.emit(assetOrProgress);
                    }
                },
                error: error => {
                    if (!Types.is(error, UploadCanceled)) {
                        this.dialogs.notifyError(error);
                    }

                    this.loadError.emit(error);
                },
            });
    }

    protected updateFile(files: ReadonlyArray<HTTP.UploadFile>) {
        const asset = this.asset();
        if (files.length !== 1 || !asset?.canUpload) {
            return;
        }

        this.dialogs.confirm('i18n:assets.replaceConfirmTitle', 'i18n:assets.replaceConfirmText')
            .subscribe(confirmed => {
                if (!confirmed) {
                    return;
                }

                this.progress.set(1);

                this.assetUploader.uploadAsset(asset, files[0])
                    .subscribe({
                        next: assetOrProgress => {
                            if (Types.isNumber(assetOrProgress)) {
                                this.progress.set(assetOrProgress);
                            } else {
                                this.loadDone.emit(assetOrProgress);
                            }
                        },
                        error: error => {
                            this.dialogs.notifyError(error);

                            this.progress.set(0);
                        },
                        complete: () => {
                            this.progress.set(0);
                        },
                    });
            });
    }

    protected emitEdit() {
        const asset = this.asset();
        if (!asset || this.isDisabled()) {
            return;
        }

        this.edit.emit(asset);
    }
}
