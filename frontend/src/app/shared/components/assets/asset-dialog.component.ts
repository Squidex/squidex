/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, QueryList, ViewChildren } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AnnotateAssetDto, AnnotateAssetForm, AppsState, AssetDto, AssetsState, AssetUploaderState, AuthService, DialogService, Types, UploadCanceled } from '@app/shared/internal';
import { AssetsService } from '@app/shared/services/assets.service';
import { AssetPathItem, ROOT_ITEM } from '@app/shared/state/assets.state';
import { AssetTextEditorComponent } from './asset-text-editor.component';
import { ImageCropperComponent } from './image-cropper.component';
import { ImageFocusPointComponent } from './image-focus-point.component';

@Component({
    selector: 'sqx-asset-dialog[allTags][asset]',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
})
export class AssetDialogComponent implements OnChanges {
    @Output()
    public complete = new EventEmitter();

    @Output()
    public changed = new EventEmitter<AssetDto>();

    @Input()
    public asset!: AssetDto;

    @Input()
    public allTags!: ReadonlyArray<string>;

    @ViewChildren(ImageCropperComponent)
    public imageCropper!: QueryList<ImageCropperComponent>;

    @ViewChildren(ImageFocusPointComponent)
    public imageFocus!: QueryList<ImageFocusPointComponent>;

    @ViewChildren(AssetTextEditorComponent)
    public textEditor!: QueryList<AssetTextEditorComponent>;

    public path!: Observable<ReadonlyArray<AssetPathItem>>;

    public selectedTab = 0;
    public isEditable = false;
    public isEditableAny = false;
    public isUploadable = false;

    public progress = 0;

    public annotateForm = new AnnotateAssetForm();

    public get isImage() {
        return this.asset.type === 'Image';
    }

    public get isVideo() {
        return this.asset.type === 'Video';
    }

    public get isDocumentLikely() {
        return this.asset.type === 'Unknown' && this.asset.fileSize < 100_000;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsState: AssetsState,
        private readonly assetUploader: AssetUploaderState,
        private readonly assetsService: AssetsService,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService,
        public readonly authService: AuthService,
    ) {
    }

    public ngOnChanges() {
        this.selectTab(0);

        this.isEditable = this.asset.canUpdate;
        this.isUploadable = this.asset.canUpload;

        this.annotateForm.load(this.asset);
        this.annotateForm.setEnabled(this.isEditable);

        this.path =
            this.assetsService.getAssetFolders(this.appsState.appName, this.asset.parentId, 'Path').pipe(
                map(folders => [ROOT_ITEM, ...folders.path]));
    }

    public navigate(id: string) {
        this.assetsState.navigate(id);
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public generateSlug() {
        this.annotateForm.generateSlug(this.asset);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public cropImage() {
        if (!this.isUploadable) {
            return;
        }

        this.uploadEdited(this.imageCropper.first.toFile());
    }

    public saveText() {
        if (!this.isUploadable) {
            return;
        }

        this.uploadEdited(this.textEditor.first.toFile());
    }

    public uploadEdited(fileChange: Promise<Blob | null>) {
        fileChange.then(file => {
            if (file) {
                this.setProgress(0);

                this.assetUploader.uploadAsset(this.asset, file)
                    .subscribe({
                        next: dto => {
                            if (Types.isNumber(dto)) {
                                this.setProgress(dto);
                            } else {
                                this.changed.emit(dto);

                                this.dialogs.notifyInfo('i18n:assets.updated');
                            }
                        },
                        error: error => {
                            if (!Types.is(error, UploadCanceled)) {
                                this.dialogs.notifyError(error);
                            }
                        },
                        complete: () => {
                        this.setProgress(0);
                        },
                    });
            } else {
                this.dialogs.notifyInfo('i18n:common.nothingChanged');
            }
        });
    }

    public setFocusPoint() {
        if (!this.isEditable) {
            return;
        }

        this.annoateAssetInternal(this.imageFocus.first.submit(this.asset));
    }

    public annotateAsset() {
        if (!this.isEditable) {
            return;
        }

        this.annoateAssetInternal(this.annotateForm.submit(this.asset));
    }

    private annoateAssetInternal(value: AnnotateAssetDto | null) {
        if (value) {
            this.assetsState.updateAsset(this.asset, value)
                .subscribe({
                    next: () => {
                        this.annotateForm.submitCompleted({ noReset: true });

                        this.dialogs.notifyInfo('i18n:assets.updated');
                    },
                    error: error => {
                        this.annotateForm.submitFailed(error);
                    },
                });
        } else {
            this.dialogs.notifyInfo('i18n:common.nothingChanged');
        }
    }

    public setProgress(progress: number) {
        this.progress = progress;

        this.changeDetector.markForCheck();
    }
}
