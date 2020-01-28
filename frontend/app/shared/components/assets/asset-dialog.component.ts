/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AnnotateAssetForm,
    AssetDto,
    AssetsState
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-dialog',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetDialogComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public asset: AssetDto;

    @Input()
    public allTags: ReadonlyArray<string>;

    public isEditable = false;

    public selectableTabs: ReadonlyArray<string> = ['Image', 'Metadata'];
    public selectedTab = this.selectableTabs[0];

    public annotateForm = new AnnotateAssetForm(this.formBuilder);

    constructor(
        private readonly assetsState: AssetsState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.isEditable = this.asset.canUpdate;

        this.annotateForm.load(this.asset);
        this.annotateForm.setEnabled(this.isEditable);
    }

    public selectTab(tab: string) {
        this.selectedTab = tab;
    }

    public generateSlug() {
        this.annotateForm.generateSlug(this.asset);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public annotateAsset() {
        if (!this.isEditable) {
            return;
        }

        const value = this.annotateForm.submit(this.asset);

        if (value) {
            this.assetsState.updateAsset(this.asset, value)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.annotateForm.submitFailed(error);
                });
        } else if (this.annotateForm.form.valid) {
            this.emitComplete();
        }
    }
}