/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetFolderDto, AssetsState, RenameAssetFolderForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-folder-dialog',
    styleUrls: ['./asset-folder-dialog.component.scss'],
    templateUrl: './asset-folder-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
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
        this.editForm.submit(async value => {
            const assetFolder = this.assetFolder();
            if (assetFolder) {
                await firstValueFrom(this.assetsState.updateAssetFolder(assetFolder, value));
            } else {
                await firstValueFrom(this.assetsState.createAssetFolder(value.folderName));
            }

            this.emitClose();
        });
    }
}
