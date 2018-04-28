/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppsState,
    AssetDto,
    AssetsService,
    AuthService,
    DateTime,
    DialogService,
    fadeAnimation,
    ModalView,
    RenameAssetForm,
    UpdateAssetDto,
    Versioned
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent implements OnInit {
    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public removeMode = false;

    @Input()
    public isDisabled = false;

    @Input()
    public isSelected = false;

    @Input()
    public isSelectable = false;

    @Output()
    public loaded = new EventEmitter<AssetDto>();

    @Output()
    public removing = new EventEmitter<AssetDto>();

    @Output()
    public updated = new EventEmitter<AssetDto>();

    @Output()
    public deleting = new EventEmitter<AssetDto>();

    @Output()
    public selected = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public renameDialog = new ModalView();
    public renameForm = new RenameAssetForm(this.formBuilder);

    public progress = 0;

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.assetsService.uploadFile(this.appsState.appName, initFile, this.authState.user!.token, DateTime.now())
                .subscribe(dto => {
                    if (dto instanceof AssetDto) {
                        this.emitLoaded(dto);
                    } else {
                        this.progress = dto;
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.emitFailed(error);
                });
        } else {
            this.updateAsset(this.asset, false);
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.assetsService.replaceFile(this.appsState.appName, this.asset.id, files[0], this.asset.version)
                .subscribe(dto => {
                    if (dto instanceof Versioned) {
                        this.updateAsset(this.asset.update(dto.payload, this.authState.user!.token, dto.version), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.dialogs.notifyError(error);

                    this.setProgress();
                });
        }
    }

    public renameAsset() {
        const value = this.renameForm.submit();

        if (value) {
            const requestDto = new UpdateAssetDto(value.name);

            this.assetsService.putAsset(this.appsState.appName, this.asset.id, requestDto, this.asset.version)
                .subscribe(dto => {
                    this.updateAsset(this.asset.rename(requestDto.fileName, this.authState.user!.token, dto.version), true);

                    this.renameForm.submitCompleted();
                    this.renameDialog.hide();
                }, error => {
                    this.dialogs.notifyError(error);

                    this.renameForm.submitFailed(error);
                });
        }
    }

    public cancelRenameAsset() {
        this.renameForm.submitCompleted();
        this.renameDialog.hide();
    }

    private setProgress(progress = 0) {
        this.progress = progress;
    }

    private emitFailed(error: any) {
        this.failed.emit(error);
    }

    private emitLoaded(asset: AssetDto) {
        this.loaded.emit(asset);
    }

    private emitUpdated(asset: AssetDto) {
        this.updated.emit(asset);
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.renameForm.load({ name: asset.fileName });
        this.asset = asset;
        this.progress = 0;

        if (emitEvent) {
            this.emitUpdated(asset);
        }

        this.cancelRenameAsset();
    }
}