/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AssetsState, RenameAssetTagForm } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-tag-dialog[tagName]',
    styleUrls: ['./asset-tag-dialog.component.scss'],
    templateUrl: './asset-tag-dialog.component.html',
})
export class AssetTagDialogComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public tagName!: string;

    public editForm = new RenameAssetTagForm();

    constructor(
        private readonly assetsState: AssetsState,
    ) {
    }

    public ngOnInit() {
        this.editForm.load({ tagName: this.tagName });
    }

    public emitComplete() {
        this.complete.emit();
    }

    public renameAssetTag() {
        const value = this.editForm.submit();

        if (!value) {
            return;
        }

        if (value.tagName === this.tagName) {
            this.emitComplete();
        }

        this.assetsState.renameTag(this.tagName, value?.tagName)
            .subscribe({
                next: () => {
                    this.emitComplete();
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }
}
