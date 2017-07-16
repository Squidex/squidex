/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

import { AppComponentBase } from './app.component-base';

import {
    ApiUrlConfig,
    AppsStoreService,
    AssetCreatedDto,
    AssetDto,
    AssetReplacedDto,
    AssetsService,
    AuthService,
    DateTime,
    fadeAnimation,
    FileHelper,
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
    private version: Version;

    public renameDialog = new ModalView();
    public renameFormSubmitted = false;
    public renameForm: FormGroup =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]]
        });

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

    public progress = 0;
    public previewUrl: string;
    public fileUrl: string;
    public fileName: string;
    public fileType: string;
    public fileIcon: string;
    public fileInfo: string;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly formBuilder: FormBuilder,
        private readonly assetsService: AssetsService,
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super(notifications, apps);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.uploadFile(app, initFile))
                .subscribe(result => {
                    if (result instanceof AssetCreatedDto) {
                        const me = `subject:${this.authService.user!.id}`;

                        const asset = new AssetDto(
                            result.id,
                            me, me,
                            DateTime.now(),
                            DateTime.now(),
                            result.fileName,
                            result.fileSize,
                            result.fileVersion,
                            result.mimeType,
                            result.isImage,
                            result.pixelWidth,
                            result.pixelHeight,
                            result.version);
                        this.loaded.emit(asset);
                    } else {
                        this.progress = result;
                    }
                }, error => {
                    this.failed.emit();

                    this.notifyError(error);
                });
        } else {
            this.updateAsset(this.asset, false);
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.replaceFile(app, this.asset.id, files[0], this.version))
                .subscribe(result => {
                    if (result instanceof AssetReplacedDto) {
                        const me = `subject:${this.authService.user!.id}`;

                        const asset = new AssetDto(
                            this.asset.id,
                            this.asset.createdBy, me,
                            this.asset.created, DateTime.now(),
                            this.asset.fileName,
                            result.fileSize,
                            result.fileVersion,
                            result.mimeType,
                            result.isImage,
                            result.pixelWidth,
                            result.pixelHeight,
                            result.version);
                        this.updateAsset(asset, true);
                    } else {
                        this.progress = result;
                    }
                }, error => {
                    this.progress = 0;

                    this.notifyError(error);
                });
        }
    }

    public renameAsset() {
        this.renameFormSubmitted = true;

        if (this.renameForm.valid) {
            this.renameForm.disable();

            const requestDto = new UpdateAssetDto(this.renameForm.controls['name'].value);

            this.appNameOnce()
                .switchMap(app => this.assetsService.putAsset(app, this.asset.id, requestDto, this.version))
                .subscribe(_ => {
                    const me = `subject:${this.authService.user!.id}`;

                    const asset = new AssetDto(
                        this.asset.id,
                        this.asset.createdBy, me,
                        this.asset.created, DateTime.now(), requestDto.fileName,
                        this.asset.fileSize,
                        this.asset.fileVersion,
                        this.asset.mimeType,
                        this.asset.isImage,
                        this.asset.pixelWidth,
                        this.asset.pixelHeight,
                        this.asset.version);

                    this.updateAsset(asset, true);
                    this.resetRename();
                }, error => {
                    this.notifyError(error);
                    this.resetRename();
                });
        }
    }

    public resetRename() {
        this.renameForm.enable();
        this.renameForm.controls['name'].setValue(this.asset.fileName);
        this.renameFormSubmitted = false;
        this.renameDialog.hide();
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;
        this.fileUrl = FileHelper.assetUrl(this.apiUrl, asset);
        this.fileInfo = FileHelper.assetInfo(asset);
        this.fileName = FileHelper.assetName(asset);
        this.fileType = FileHelper.fileType(asset.mimeType, this.asset.fileName);
        this.fileIcon = FileHelper.fileIcon(asset.mimeType);
        this.progress = 0;
        this.previewUrl = FileHelper.assetPreviewUrl(this.apiUrl, asset);
        this.version = asset.version;

        if (emitEvent) {
            this.updated.emit(asset);
        }

        this.resetRename();
    }
}