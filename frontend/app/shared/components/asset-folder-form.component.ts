/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { AssetFolderForm, AssetsState } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-form',
    styleUrls: ['./asset-folder-form.component.scss'],
    templateUrl: './asset-folder-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetFolderFormComponent {
    @Output()
    public complete = new EventEmitter();

    public createForm = new AssetFolderForm(this.formBuilder);

    constructor(
        private readonly assetsState: AssetsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public emitComplete() {
        this.complete.emit();
    }

    public createAssetFolder() {
        const value = this.createForm.submit();

        if (value) {
            this.assetsState.createFolder(value.folderName)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.createForm.submitFailed(error);
                });
        }
    }
}