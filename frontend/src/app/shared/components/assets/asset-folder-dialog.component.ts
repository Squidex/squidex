/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, input, output, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetFolderDto, AssetsState, RenameAssetFolderForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-dialog',
    styleUrls: ['./asset-folder-dialog.component.scss'],
    templateUrl: './asset-folder-dialog.component.html',
    imports: [
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
    private readonly assetsState = inject(AssetsState);

    /** Emits when the dialog is closed or the folder has been saved. */
    public readonly dialogClose = output();

    /** The folder to rename. Creates a new folder when not set. */
    public readonly assetFolder = input<AssetFolderDto>();

    protected readonly editForm = new RenameAssetFolderForm();

    constructor() {
        effect(() => {
            const assetFolder = this.assetFolder();
            if (assetFolder) {
                this.editForm.load(assetFolder);
            }
        });
    }

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected createAssetFolder() {
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
