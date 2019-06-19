/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AnnotateAssetForm,
    AppsState,
    AssetDto,
    AssetsService,
    StatefulComponent
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-dialog',
    styleUrls: ['./asset-dialog.component.scss'],
    templateUrl: './asset-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetDialogComponent extends StatefulComponent implements OnInit {
    @Input()
    public asset: AssetDto;

    @Input()
    public allTags: string[];

    @Output()
    public cancel = new EventEmitter();

    @Output()
    public complete = new EventEmitter<AssetDto>();

    public isEditable = false;

    public annotateForm = new AnnotateAssetForm(this.formBuilder);

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly formBuilder: FormBuilder
    ) {
        super(changeDetector, {
            isRenaming: false,
            isTagging: false,
            progress: 0
        });
    }

    public ngOnInit() {
        this.isEditable = this.asset.canUpdate;

        this.annotateForm.load(this.asset);
        this.annotateForm.setEnabled(this.isEditable);
    }

    public generateSlug() {
        this.annotateForm.generateSlug(this.asset);
    }

    public emitCancel() {
        this.cancel.emit();
    }

    public emitComplete(asset: AssetDto) {
        this.complete.emit(asset);
    }

    public annotateAsset() {
        if (!this.isEditable) {
            return;
        }

        const value = this.annotateForm.submit(this.asset);

        if (value) {
            this.assetsService.putAsset(this.appsState.appName, this.asset, value, this.asset.version)
                .subscribe(dto => {
                    this.emitComplete(dto);
                }, error => {
                    this.annotateForm.submitFailed(error);
                });
        }
    }
}