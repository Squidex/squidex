/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ConfirmClickDirective, ControlErrorsComponent, CopyDirective, DialogService, FormErrorComponent, FormHintComponent, HTTP, ModalDialogComponent, ProgressBarComponent, switchMapCached, TagEditorComponent, TooltipDirective, TransformInputDirective, TranslatePipe, Types, VideoPlayerComponent } from '@app/framework';
import { AnnotateAssetDto, AnnotateAssetForm, AppsState, AssetDto, AssetPathItem, AssetsService, AssetsState, AssetUploaderState, AuthService, MoveAssetForm, MoveAssetItemDto, ROOT_ITEM, UploadCanceled } from '@app/shared/internal';
import { AssetFolderDropdownComponent } from './asset-folder-dropdown.component';
import { AssetHistoryComponent } from './asset-history.component';
import { AssetPathComponent } from './asset-path.component';
import { AssetTextEditorComponent } from './asset-text-editor.component';
import { ImageCropperComponent } from './image-cropper.component';
import { ImageFocusPointComponent } from './image-focus-point.component';
import { AssetPreviewUrlPipe, AssetUrlPipe, PreviewableType } from './pipes';

@Component({
    standalone: true,
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
        AsyncPipe,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        FormErrorComponent,
        FormHintComponent,
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
export class AssetDialogComponent implements OnInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Output()
    public assetReplaced = new EventEmitter<AssetDto>();

    @Output()
    public assetUpdated = new EventEmitter<AssetDto>();

    @Input({ required: true })
    public asset!: AssetDto;

    @ViewChildren(ImageCropperComponent)
    public imageCropper!: QueryList<ImageCropperComponent>;

    @ViewChildren(ImageFocusPointComponent)
    public imageFocus!: QueryList<ImageFocusPointComponent>;

    @ViewChildren(AssetTextEditorComponent)
    public textEditor!: QueryList<AssetTextEditorComponent>;

    public pathSource = new BehaviorSubject<string>('');
    public pathItems!: Observable<ReadonlyArray<AssetPathItem>>;

    public progress = 0;

    public selectedTab = 0;
    public isEditable = false;
    public isEditableAny = false;
    public isUploadable = false;
    public isMoving = false;
    public isMoveable = false;

    public moveForm = new MoveAssetForm();

    public annotateTags!: Observable<string[]>;
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

        this.pathItems =
            this.pathSource.pipe(
                switchMapCached(x => this.assetsService.getAssetFolders(this.appsState.appName, x, 'Path')), map(({ path }) => [ROOT_ITEM, ...path]));

        this.selectTab(0);

        this.assetchanged(this.asset);
    }

    private assetchanged(asset: AssetDto) {
        this.pathSource.next(asset.parentId);

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

    public uploadEdited(fileChange: Promise<HTTP.UploadFile | null>) {
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
