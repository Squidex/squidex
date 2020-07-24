/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, QueryList, ViewChildren } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AnnotateAssetDto, AnnotateAssetForm, AssetDto, AssetsState, AssetUploaderState, AuthService, DialogService, Types, UploadCanceled } from '@app/shared/internal';
import { ImageCropperComponent } from './image-cropper.component';
import { ImageFocusPointComponent } from './image-focus-point.component';

const TABS_IMAGE: ReadonlyArray<string> = [
    'i18n:assets.metadata',
    'i18n:assets.image',
    'i18n:assets.focusPoint',
    'i18n:assets.history'
];

const TABS_DEFAULT: ReadonlyArray<string> = [
    'i18n:assets.metadata',
    'i18n:assets.history'
];

@Component({
    selector: 'sqx-asset-dialog',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetDialogComponent implements OnChanges {
    @Output()
    public complete = new EventEmitter();

    @Output()
    public changed = new EventEmitter<AssetDto>();

    @Input()
    public asset: AssetDto;

    @Input()
    public allTags: ReadonlyArray<string>;

    @ViewChildren(ImageCropperComponent)
    public imageCropper: QueryList<ImageCropperComponent>;

    @ViewChildren(ImageFocusPointComponent)
    public imageFocus: QueryList<ImageFocusPointComponent>;

    public isEditable = false;
    public isEditableAny = false;
    public isUploadable = false;

    public progress = 0;

    public selectableTabs: ReadonlyArray<string>;
    public selectedTab: string;

    public annotateForm = new AnnotateAssetForm(this.formBuilder);

    constructor(
        private readonly assetsState: AssetsState,
        private readonly assetUploader: AssetUploaderState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        public readonly authService: AuthService
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.asset.canUpdate;
        this.isUploadable = this.asset.canUpload;

        this.annotateForm.load(this.asset);
        this.annotateForm.setEnabled(this.isEditable);

        if (this.asset.type === 'Image') {
            this.selectableTabs = TABS_IMAGE;
        } else {
            this.selectableTabs = TABS_DEFAULT;
        }

        if (this.selectableTabs.indexOf(this.selectedTab) < 0) {
            this.selectTab(this.selectableTabs[0]);
        }
    }

    public selectTab(tab: string) {
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

        this.imageCropper.first.toFile().then(file => {
            if (file) {
                this.setProgress(0);

                this.assetUploader.uploadAsset(this.asset, file)
                    .subscribe(dto => {
                        if (Types.isNumber(dto)) {
                            this.setProgress(dto);
                        } else {
                            this.changed.emit(dto);

                            this.dialogs.notifyInfo('i18n:assets.updated');
                        }
                    }, error => {
                        if (!Types.is(error, UploadCanceled)) {
                            this.dialogs.notifyError(error);
                        }
                    }, () => {
                        this.setProgress(0);
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
                .subscribe(() => {
                    this.annotateForm.submitCompleted({ noReset: true });

                    this.dialogs.notifyInfo('i18n:assets.updated');
                }, error => {
                    this.annotateForm.submitFailed(error);
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