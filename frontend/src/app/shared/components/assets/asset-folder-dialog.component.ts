/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, effect, input, output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetFolderDto, AssetsState, RenameAssetFolderForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-dialog',
    styleUrls: ['./asset-folder-dialog.component.scss'],
    templateUrl: './asset-folder-dialog.component.html',
    imports: [
        AsyncPipe,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDialogComponent {
    /** Emits when the dialog is closed or the folder has been saved. */
    public readonly dialogClose = output();

    /** The folder to rename. Creates a new folder when not set. */
    public readonly assetFolder = input<AssetFolderDto>();

    public readonly editForm = new RenameAssetFolderForm();

    constructor(
        private readonly assetsState: AssetsState,
    ) {
        effect(() => {
            const assetFolder = this.assetFolder();
            if (assetFolder) {
                this.editForm.load(assetFolder);
            }
        });
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createAssetFolder() {
        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        const assetFolder = this.assetFolder();
        const request = assetFolder ?
            this.assetsState.updateAssetFolder(assetFolder, value) :
            this.assetsState.createAssetFolder(value.folderName);

        request.subscribe({
            next: () => {
                this.emitClose();
            },
            error: error => {
                this.editForm.submitFailed(error);
            },
        });
    }
}
