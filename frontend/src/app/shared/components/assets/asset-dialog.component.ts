/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AnnotateAssetDto, AnnotateAssetForm, AppsState, AssetDto, AssetsState, AssetUploaderState, AuthService, DialogService, MoveAssetForm, Types, UploadCanceled } from '@app/shared/internal';
import { AssetsService, MoveAssetItemDto } from '@app/shared/services/assets.service';
import { AssetPathItem, ROOT_ITEM } from '@app/shared/state/assets.state';
import { AssetTextEditorComponent } from './asset-text-editor.component';
import { ImageCropperComponent } from './image-cropper.component';
import { ImageFocusPointComponent } from './image-focus-point.component';

@Component({
    selector: 'sqx-asset-dialog[asset]',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
})
export class AssetDialogComponent implements OnInit {
    private readonly pathCache: { [parentId: string]: ReadonlyArray<AssetPathItem> } = {};

    @Output()
    public complete = new EventEmitter();

    @Output()
    public assetReplaced = new EventEmitter<AssetDto>();

    @Output()
    public assetUpdated = new EventEmitter<AssetDto>();

    @Input()
    public asset!: AssetDto;

    @ViewChildren(ImageCropperComponent)
    public imageCropper!: QueryList<ImageCropperComponent>;

    @ViewChildren(ImageFocusPointComponent)
    public imageFocus!: QueryList<ImageFocusPointComponent>;

    @ViewChildren(AssetTextEditorComponent)
    public textEditor!: QueryList<AssetTextEditorComponent>;

    public path: ReadonlyArray<AssetPathItem> = [];

    public progress = 0;

    public selectedTab = 0;
    public isEditable = false;
    public isEditableAny = false;
    public isUploadable = false;
    public isMoving = false;
    public isMoveable = false;

    public moveForm = new MoveAssetForm();

    public annotateTag: ReadonlyArray<string> = [];
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

    public ngOnInit() {
        this.annotateTags =
            this.assetsService.getTags(this.appsState.appName).pipe(
                map(tags => Object.keys(tags)));

        this.selectTab(0);

        this.assetchanged(this.asset);
    }

    private assetchanged(asset: AssetDto) {
        const cachedPath = 
        this.path =
            this.assetsService.getAssetFolders(this.appsState.appName, asset.parentId, 'Path').pipe(
                map(folders => [ROOT_ITEM, ...folders.path]));

        this.isEditable = asset.canUpdate;
        this.isUploadable = asset.canUpload;
        this.isMoveable = asset.canMove;

        this.annotateForm.load(asset);
        this.annotateForm.setEnabled(this.isEditable);

        this.moveForm.load(asset);
        this.moveForm.setEnabled(this.isMoveable);

        this.asset = asset;

    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public navigate(id: string) {
        this.assetsState.navigate(id);
    }

    public generateSlug() {
        this.annotateForm.generateSlug(this.asset);
    }

    public startMoving() {
        this.isMoving = true;
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
                                this.assetReplaced.emit(dto);
                                this.assetchanged(dto);

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

        this.annotateInternal(this.imageFocus.first.submit(this.asset));
    }

    public annotateAsset() {
        if (!this.isEditable) {
            return;
        }

        this.annotateInternal(this.annotateForm.submit(this.asset));
    }

    public moveAsset() {
        if (!this.isMoveable) {
            return;
        }

        this.moveInternal(this.moveForm.submit());
    }

    private annotateInternal(value: AnnotateAssetDto | null) {
        if (!value) {
            this.dialogs.notifyInfo('i18n:common.nothingChanged');
            return;
        }

        this.assetsState.updateAsset(this.asset, value)
            .subscribe({
                next: dto => {
                    this.assetUpdated.emit(dto);
                    this.assetchanged(dto);

                    this.annotateForm.submitCompleted({ noReset: true });

                    this.dialogs.notifyInfo('i18n:assets.updated');
                },
                error: error => {
                    this.annotateForm.submitFailed(error);
                },
            });
    }

    private moveInternal(values: MoveAssetItemDto | null) {
        if (!values) {
            this.isMoving = false;
            return;
        }

        this.assetsState.moveAsset(this.asset, values.parentId)
            .subscribe({
                next: (dto) => {
                    this.assetUpdated.emit(dto);
                    this.assetchanged(dto);

                    this.annotateForm.submitCompleted({ noReset: true });

                    this.dialogs.notifyInfo('i18n:assets.moved');
                },
                complete: () => {
                    this.isMoving = false;
                },
            });
    }

    public setProgress(progress: number) {
        this.progress = progress;

        this.changeDetector.markForCheck();
    }
}
