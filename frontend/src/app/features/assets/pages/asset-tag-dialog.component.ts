/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AssetsState, RenameAssetTagForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-tag-dialog',
    styleUrls: ['./asset-tag-dialog.component.scss'],
    templateUrl: './asset-tag-dialog.component.html',
})
export class AssetTagDialogComponent implements OnInit {
    @Output()
    public close = new EventEmitter();

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
        this.close.emit();
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
