/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, ModalDialogComponent, TooltipDirective, TranslatePipe } from '@app/framework';
import { AssetFolderDto, AssetsState, RenameAssetFolderForm } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-asset-folder-dialog',
    styleUrls: ['./asset-folder-dialog.component.scss'],
    templateUrl: './asset-folder-dialog.component.html',
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class AssetFolderDialogComponent implements OnInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Input()
    public assetFolder?: AssetFolderDto;

    public editForm = new RenameAssetFolderForm();

    constructor(
        private readonly assetsState: AssetsState,
    ) {
    }

    public ngOnInit() {
        if (this.assetFolder) {
            this.editForm.load(this.assetFolder);
        }
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createAssetFolder() {
        const value = this.editForm.submit();

        if (value) {
            if (this.assetFolder) {
                this.assetsState.updateAssetFolder(this.assetFolder, value)
                    .subscribe({
                        next: () => {
                            this.emitClose();
                        },
                        error: error => {
                            this.editForm.submitFailed(error);
                        },
                    });
            } else {
                this.assetsState.createAssetFolder(value.folderName)
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
    }
}
