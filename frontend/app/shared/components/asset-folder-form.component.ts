/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AssetFolderDto,
    AssetFolderForm,
    AssetsState
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-form',
    styleUrls: ['./asset-folder-form.component.scss'],
    templateUrl: './asset-folder-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetFolderFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public assetFolder: AssetFolderDto;

    public editForm = new AssetFolderForm(this.formBuilder);

    constructor(
        private readonly assetsState: AssetsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        if (this.assetFolder) {
            this.editForm.load({ folderName: this.assetFolder.folderName });
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
                    .subscribe(() => {
                        this.emitComplete();
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            } else {
                this.assetsState.createAssetFolder(value.folderName)
                    .subscribe(() => {
                        this.emitComplete();
                    }, error => {
                        this.editForm.submitFailed(error);
                    });
            }
        }
    }
}