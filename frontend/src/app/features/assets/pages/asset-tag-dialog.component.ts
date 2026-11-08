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
import { FocusOnInitDirective, FormErrorComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { AssetsState, FormRowComponent, RenameAssetTagForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-tag-dialog',
    styleUrls: ['./asset-tag-dialog.component.scss'],
    templateUrl: './asset-tag-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FocusOnInitDirective,
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetTagDialogComponent {
    private readonly assetsState = inject(AssetsState);

    /** Emits when the dialog is closed, after a rename or when canceled. */
    public readonly dialogClose = output();

    /** The name of the tag to rename. */
    public readonly tagName = input.required<string>();

    protected readonly editForm = new RenameAssetTagForm();

    constructor() {
        effect(() => {
            this.editForm.load({ tagName: this.tagName() });
        });
    }

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected renameAssetTag() {
        this.editForm.submit(async value => {
            if (value.tagName !== this.tagName()) {
                await firstValueFrom(this.assetsState.renameTag(this.tagName(), value.tagName));
            }

            this.emitClose();
        });
    }
}
