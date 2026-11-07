/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, input, output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FocusOnInitDirective, FormErrorComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { AssetsState, FormRowComponent, RenameAssetTagForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-tag-dialog',
    styleUrls: ['./asset-tag-dialog.component.scss'],
    templateUrl: './asset-tag-dialog.component.html',
    imports: [
        FocusOnInitDirective,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetTagDialogComponent {
    /** Emits when the dialog is closed, after a rename or when canceled. */
    public readonly dialogClose = output();

    /** The name of the tag to rename. */
    public readonly tagName = input.required<string>();

    protected readonly editForm = new RenameAssetTagForm();

    constructor(
        private readonly assetsState: AssetsState,
    ) {
        effect(() => {
            this.editForm.load({ tagName: this.tagName() });
        });
    }

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected renameAssetTag() {
        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        if (value.tagName === this.tagName()) {
            this.emitClose();
            return;
        }

        this.assetsState.renameTag(this.tagName(), value.tagName)
            .subscribe({
                next: () => {
                    this.emitClose();
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }
}
