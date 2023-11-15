/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, FocusOnInitDirective, FormErrorComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { AssetsState, RenameAssetTagForm } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-asset-tag-dialog',
    styleUrls: ['./asset-tag-dialog.component.scss'],
    templateUrl: './asset-tag-dialog.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormErrorComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetTagDialogComponent implements OnInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Input({ required: true })
    public tagName!: string;

    public editForm = new RenameAssetTagForm();

    constructor(
        private readonly assetsState: AssetsState,
    ) {
    }

    public ngOnInit() {
        this.editForm.load({ tagName: this.tagName });
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public renameAssetTag() {
        const value = this.editForm.submit();

        if (!value) {
            return;
        }

        if (value.tagName === this.tagName) {
            this.emitClose();
        }

        this.assetsState.renameTag(this.tagName, value?.tagName)
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
