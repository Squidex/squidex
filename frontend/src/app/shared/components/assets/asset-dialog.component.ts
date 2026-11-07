/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input, linkedSignal, output, signal, viewChild } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { map } from 'rxjs/operators';
import { ConfirmClickDirective, ControlErrorsComponent, CopyDirective, DialogService, FormErrorComponent, FormRowComponent, HTTP, ModalDialogComponent, ProgressBarComponent, switchMapCached, TagEditorComponent, TooltipDirective, TransformInputDirective, TranslatePipe, Types, VideoPlayerComponent } from '@app/framework';
import { AnnotateAssetDto, AnnotateAssetForm, AppsState, AssetDto, AssetsService, AssetsState, AssetUploaderState, MoveAssetDto, MoveAssetForm, ROOT_ITEM, UploadCanceled } from '@app/shared/internal';
import { AssetFolderDropdownComponent } from './asset-folder-dropdown.component';
import { AssetHistoryComponent } from './asset-history.component';
import { AssetPathComponent } from './asset-path.component';
import { AssetTextEditorComponent } from './asset-text-editor.component';
import { ImageCropperComponent } from './image-cropper.component';
import { ImageFocusPointComponent } from './image-focus-point.component';
import { AssetPreviewUrlPipe, AssetUrlPipe, PreviewableType } from './pipes';

@Component({
    selector: 'sqx-asset-dialog',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
    imports: [
        AssetFolderDropdownComponent,
        AssetHistoryComponent,
        AssetPathComponent,
        AssetPreviewUrlPipe,
        AssetTextEditorComponent,
        AssetUrlPipe,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        ImageCropperComponent,
        ImageFocusPointComponent,
        ModalDialogComponent,
        NgxDocViewerModule,
        PreviewableType,
        ProgressBarComponent,
        ReactiveFormsModule,
        RouterLink,
        TagEditorComponent,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
        VideoPlayerComponent,
    ],
})
export class AssetDialogComponent {
    private readonly assetsState = inject(AssetsState);
    private readonly assetUploader = inject(AssetUploaderState);
    private readonly dialogs = inject(DialogService);

    private readonly imageCropper = viewChild(ImageCropperComponent);
    private readonly imageFocus = viewChild(ImageFocusPointComponent);
    private readonly textEditor = viewChild(AssetTextEditorComponent);

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** Emits when the file of the asset has been replaced. */
    public readonly assetReplaced = output<AssetDto>();

    /** Emits when the metadata of the asset has been updated or the asset has been moved. */
    public readonly assetUpdated = output<AssetDto>();

    /** The asset to edit. */
    public readonly asset = input.required<AssetDto>();

    protected readonly assetActual = linkedSignal(() => this.asset());

    protected readonly assetPath;
    protected readonly assetTags;

    protected readonly progress = signal(0);

    protected readonly selectedTab = signal(0);

    protected readonly isMoving = signal(false);

    protected readonly isEditable = computed(() => {
        return this.assetActual().canUpdate;
    });

    protected readonly isUploadable = computed(() => {
        return this.assetActual().canUpload;
    });

    protected readonly isMoveable = computed(() => {
        return this.assetActual().canMove;
    });

    protected readonly isImage = computed(() => {
        return this.assetActual().type === 'Image';
    });

    protected readonly isVideo = computed(() => {
        return this.assetActual().type === 'Video';
    });

    protected readonly isDocumentLikely = computed(() => {
        const asset = this.assetActual();

        return asset.type === 'Unknown' && asset.fileSize < 100_000;
    });

    protected readonly moveForm = new MoveAssetForm();

    protected readonly annotateForm = new AnnotateAssetForm();

    constructor() {
        const appsState = inject(AppsState);
        const assetsService = inject(AssetsService);

        this.assetTags = toSignal(
            assetsService.getTags(appsState.appName).pipe(
                map(tags => Object.keys(tags))));

        this.assetPath = toSignal(
            toObservable(this.assetActual).pipe(
                map(asset => asset.parentId),
                switchMapCached(x => assetsService.getAssetFolders(appsState.appName, x, 'Path')),
                map(({ path }) => [ROOT_ITEM, ...path])),
            { initialValue: [] });

        effect(() => {
            const asset = this.assetActual();

            this.annotateForm.load(asset);
            this.annotateForm.setEnabled(asset.canUpdate);

            this.moveForm.load(asset);
            this.moveForm.setEnabled(asset.canMove);
        });
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
    }

    protected navigate(id: string) {
        this.assetsState.navigate(id);
    }

    protected generateSlug() {
        this.annotateForm.generateSlug(this.assetActual());
    }

    protected startMoving() {
        this.isMoving.set(true);
    }

    protected cropImage() {
        const imageCropper = this.imageCropper();
        if (!imageCropper || !this.isUploadable()) {
            return;
        }

        this.uploadEdited(imageCropper.toFile());
    }

    protected saveText() {
        const textEditor = this.textEditor();
        if (!textEditor || !this.isUploadable()) {
            return;
        }

        this.uploadEdited(textEditor.toFile());
    }

    private uploadEdited(fileChange: Promise<HTTP.UploadFile | null>) {
        fileChange.then(file => {
            if (!file) {
                this.dialogs.notifyInfo('i18n:common.nothingChanged');
                return;
            }

            this.progress.set(0);

            this.assetUploader.uploadAsset(this.assetActual(), file)
                .subscribe({
                    next: dto => {
                        if (Types.isNumber(dto)) {
                            this.progress.set(dto);
                        } else {
                            this.assetReplaced.emit(dto);
                            this.assetActual.set(dto);

                            this.dialogs.notifyInfo('i18n:assets.updated');
                        }
                    },
                    error: error => {
                        if (!Types.is(error, UploadCanceled)) {
                            this.dialogs.notifyError(error);
                        }
                    },
                    complete: () => {
                        this.progress.set(0);
                    },
                });
        });
    }

    protected setFocusPoint() {
        const imageFocus = this.imageFocus();
        if (!imageFocus || !this.isEditable()) {
            return;
        }

        this.annotateInternal(imageFocus.submit(this.assetActual()));
    }

    protected annotateAsset() {
        if (!this.isEditable()) {
            return;
        }

        this.annotateInternal(this.annotateForm.submit(this.assetActual()));
    }

    protected moveAsset() {
        if (!this.isMoveable()) {
            return;
        }

        this.moveInternal(this.moveForm.submit());
    }

    private annotateInternal(value: AnnotateAssetDto | null) {
        if (!value) {
            this.dialogs.notifyInfo('i18n:common.nothingChanged');
            return;
        }

        this.assetsState.updateAsset(this.assetActual(), value)
            .subscribe({
                next: dto => {
                    this.assetUpdated.emit(dto);
                    this.assetActual.set(dto);

                    this.annotateForm.submitCompleted({ noReset: true });

                    this.dialogs.notifyInfo('i18n:assets.updated');
                },
                error: error => {
                    this.annotateForm.submitFailed(error);
                },
            });
    }

    private moveInternal(values: MoveAssetDto | null) {
        if (!values) {
            this.isMoving.set(false);
            return;
        }

        this.assetsState.moveAsset(this.assetActual(), values.parentId)
            .subscribe({
                next: dto => {
                    this.assetUpdated.emit(dto);
                    this.assetActual.set(dto);

                    this.annotateForm.submitCompleted({ noReset: true });

                    this.dialogs.notifyInfo('i18n:assets.moved');
                },
                complete: () => {
                    this.isMoving.set(false);
                },
            });
    }
}
