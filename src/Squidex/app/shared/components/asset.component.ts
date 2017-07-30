/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { AppComponentBase } from './app.component-base';

import {
    AppsStoreService,
    AssetDto,
    AssetReplacedDto,
    AssetsService,
    AuthService,
    fadeAnimation,
    ModalView,
    NotificationService,
    UpdateAssetDto,
    Version
} from './../declarations-base';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent extends AppComponentBase implements OnInit {
    private assetVersion: Version;

    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public closeMode = false;

    @Output()
    public loaded = new EventEmitter<AssetDto>();

    @Output()
    public closing = new EventEmitter<AssetDto>();

    @Output()
    public updated = new EventEmitter<AssetDto>();

    @Output()
    public deleting = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public renameDialog = new ModalView();
    public renameFormSubmitted = false;
    public renameForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]]
        });

    public progress = 0;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly formBuilder: FormBuilder,
        private readonly assetsService: AssetsService,
        private readonly authService: AuthService
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.uploadFile(app, initFile, this.authService.user.token))
                .subscribe(dto => {
                    if (dto instanceof AssetDto) {
                        this.emitLoaded(dto);
                    } else {
                        this.progress = dto;
                    }
                }, error => {
                    this.notifyError(error);
                });
        } else {
            this.updateAsset(this.asset, false);
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.replaceFile(app, this.asset.id, files[0], this.assetVersion))
                .subscribe(dto => {
                    if (dto instanceof AssetReplacedDto) {
                        this.updateAsset(this.asset.update(dto, this.authService.user.token), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.setProgress();
                    this.emitFailed(error);
                });
        }
    }

    public renameAsset() {
        this.renameFormSubmitted = true;

        if (this.renameForm.valid) {
            this.renameForm.disable();

            const requestDto = new UpdateAssetDto(this.renameForm.controls['name'].value);

            this.appNameOnce()
                .switchMap(app => this.assetsService.putAsset(app, this.asset.id, requestDto, this.assetVersion))
                .subscribe(() => {
                    this.updateAsset(this.asset.rename(requestDto.fileName, this.authService.user.token), true);
                    this.resetRenameForm();
                }, error => {
                    this.notifyError(error);
                    this.enableRenameForm();
                });
        }
    }

    public cancelRenameAsset() {
        this.resetRenameForm();
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

    private enableRenameForm() {
        this.renameForm.enable();
    }

    private resetRenameForm() {
        this.renameForm.enable();
        this.renameForm.controls['name'].setValue(this.asset.fileName);
        this.renameFormSubmitted = false;
        this.renameDialog.hide();
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;
        this.assetVersion = asset.version;
        this.progress = 0;

        if (emitEvent) {
            this.emitUpdated(asset);
        }

        this.resetRenameForm();
    }
}