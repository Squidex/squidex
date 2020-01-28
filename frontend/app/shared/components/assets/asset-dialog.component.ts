/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AnnotateAssetForm,
    AssetDto,
    AssetsState,
    AssetUploaderState,
    AuthService,
    DialogService,
    Types,
    UploadCanceled
} from '@app/shared/internal';

import { ImageEditorComponent } from './image-editor.component';

@Component({
    selector: 'sqx-asset-dialog',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetDialogComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Output()
    public changed = new EventEmitter<AssetDto>();

    @Input()
    public accessToken: string;

    @Input()
    public asset: AssetDto;

    @Input()
    public allTags: ReadonlyArray<string>;

    @ViewChildren(ImageEditorComponent)
    public imageEditor: QueryList<ImageEditorComponent>;

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

    public ngOnInit() {
        this.isEditable = this.asset.canUpdate;
        this.isUploadable = this.asset.canUpload;

        this.annotateForm.load(this.asset);
        this.annotateForm.setEnabled(this.isEditable);

        if (this.asset.type === 'Image') {
            this.selectableTabs = ['Image', 'Metadata'];
        } else {
            this.selectableTabs = ['Metadata'];
        }

        this.selectTab(this.selectableTabs[0]);
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

    public annotateAsset() {
        if (this.selectedTab === 'Image') {
            if (!this.isUploadable) {
                return;
            }

            const file = this.imageEditor.first.toFile();

            if (file) {
                this.setProgress(0);

                this.assetUploader.uploadAsset(this.asset, file)
                    .subscribe(dto => {
                        if (Types.isNumber(dto)) {
                            this.setProgress(dto);
                        } else {
                            this.changed.emit(dto);
                        }

                        this.dialogs.notifyInfo('Asset has been updated.');
                    }, error => {
                        if (!Types.is(error, UploadCanceled)) {
                            this.dialogs.notifyError(error);
                        }
                    }, () => {
                        this.setProgress(0);
                    });
            } else {
                this.dialogs.notifyInfo('Nothing has changed.');
            }
        } else {
            if (!this.isEditable) {
                return;
            }

            const value = this.annotateForm.submit(this.asset);

            if (value) {
                this.assetsState.updateAsset(this.asset, value)
                    .subscribe(() => {
                        this.annotateForm.submitCompleted({ noReset: true });

                        this.dialogs.notifyInfo('Asset has been updated.');
                    }, error => {
                        this.annotateForm.submitFailed(error);
                    });
            } else {
                this.dialogs.notifyInfo('Nothing has changed.');
            }
        }
    }

    public setProgress(progress: number) {
        this.progress = progress;

        this.changeDetector.markForCheck();
    }
}