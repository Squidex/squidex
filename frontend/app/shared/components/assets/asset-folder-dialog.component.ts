/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AssetFolderDto, AssetsState, RenameAssetFolderForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-dialog',
    styleUrls: ['./asset-folder-dialog.component.scss'],
    templateUrl: './asset-folder-dialog.component.html',
})
export class AssetFolderDialogComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public assetFolder: AssetFolderDto;

    public editForm = new RenameAssetFolderForm(this.formBuilder);

    constructor(
        private readonly assetsState: AssetsState,
        private readonly formBuilder: FormBuilder,
    ) {
    }

    public ngOnInit() {
        if (this.assetFolder) {
            this.editForm.load(this.assetFolder);
        }
    }

    public emitComplete() {
        this.complete.emit();
    }

    public createAssetFolder() {
        const value = this.editForm.submit();

        if (value) {
            if (this.assetFolder) {
                this.assetsState.updateAssetFolder(this.assetFolder, value)
                    .subscribe({
                        next: () => {
                            this.emitComplete();
                        },
                        error: error => {
                            this.editForm.submitFailed(error);
                        },
                    });
            } else {
                this.assetsState.createAssetFolder(value.folderName)
                    .subscribe({
                        next: () => {
                            this.emitComplete();
                        },
                        error: error => {
                            this.editForm.submitFailed(error);
                        },
                    });
            }
        }
    }
}
