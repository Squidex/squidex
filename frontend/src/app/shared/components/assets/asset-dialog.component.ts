/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal, output, signal, viewChild } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { EMPTY, lastValueFrom } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ConfirmClickDirective, ControlErrorsComponent, CopyDirective, DialogService, FormErrorComponent, FormRowComponent, HTTP, ModalDialogComponent, ProgressBarComponent, TagEditorComponent, TooltipDirective, TransformInputDirective, TranslatePipe, Types, VideoPlayerComponent } from '@app/framework';
import { AnnotateAssetDto, AnnotateAssetForm, AppsState, AssetDto, AssetsService, AssetsState, AssetUploaderState, MoveAssetForm, ROOT_ITEM, UploadCanceled } from '@app/shared/internal';
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
    changeDetection: ChangeDetectionStrategy.OnPush,
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
        FormField,
        FormRowComponent,
        FormsModule,
        ImageCropperComponent,
        ImageFocusPointComponent,
        ModalDialogComponent,
        NgxDocViewerModule,
        PreviewableType,
        ProgressBarComponent,
        RouterLink,
        TagEditorComponent,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
        VideoPlayerComponent,
    ],
})
export class AssetDialogComponent {
    private readonly appsState = inject(AppsState);
    private readonly assetsService = inject(AssetsService);
    private readonly assetsState = inject(AssetsState);
    private readonly assetUploader = inject(AssetUploaderState);
    private readonly dialogs = inject(DialogService);

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** Emits when the file of the asset has been replaced. */
    public readonly assetReplaced = output<AssetDto>();

    /** Emits when the metadata of the asset has been updated or the asset has been moved. */
    public readonly assetUpdated = output<AssetDto>();

    /** The asset to edit. */
    public readonly asset = input.required<AssetDto>();

    protected readonly imageCropper = viewChild(ImageCropperComponent);

    protected readonly imageFocus = viewChild(ImageFocusPointComponent);

    protected readonly textEditor = viewChild(AssetTextEditorComponent);

    protected readonly assetActual = linkedSignal(() => this.asset());

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

    protected readonly assetPath = rxResource({
        params: () => this.assetActual().parentId,
        stream: ({ params }) => this.assetsService.getAssetFolders(this.appsState.appName, params, 'Path').pipe(
            map(({ path }) => [ROOT_ITEM, ...path])),
        defaultValue: [],
    });

    protected readonly assetTags = rxResource({
        stream: () => this.assetsService.getTags(this.appsState.appName).pipe(
            map(tags => Object.keys(tags))),
    });

    protected readonly moveForm = new MoveAssetForm();

    protected readonly annotateForm = new AnnotateAssetForm();

    constructor() {
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

        this.annotateInternal(imageFocus.getChanges(this.assetActual()))
            .subscribe({
                error: error => {
                    this.dialogs.notifyError(error);
                },
            });
    }

    protected annotateAsset() {
        if (!this.isEditable()) {
            return;
        }

        this.annotateForm.submitChanges(this.assetActual(), request => {
            return this.annotateInternal(request);
        });
    }

    protected moveAsset() {
        if (!this.isMoveable()) {
            return;
        }

        this.moveForm.submit(async value => {
            const dto = await lastValueFrom(this.assetsState.moveAsset(this.assetActual(), value.parentId), { defaultValue: undefined });
            if (dto) {
                this.assetUpdated.emit(dto);
                this.assetActual.set(dto);

                this.dialogs.notifyInfo('i18n:assets.moved');
            }

            this.isMoving.set(false);
        });
    }

    private annotateInternal(request: AnnotateAssetDto | null) {
        if (!request) {
            this.dialogs.notifyInfo('i18n:common.nothingChanged');
            return EMPTY;
        }

        return this.assetsState.updateAsset(this.assetActual(), request).pipe(
            tap(dto => {
                this.assetUpdated.emit(dto);
                this.assetActual.set(dto);

                this.dialogs.notifyInfo('i18n:assets.updated');
            }));
    }
}
