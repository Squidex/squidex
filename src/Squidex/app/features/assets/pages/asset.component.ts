/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    ApiUrlConfig,
    AppComponentBase,
    AppsStoreService,
    AssetCreatedDto,
    AssetDto,
    AssetReplacedDto,
    AssetsService,
    AuthService,
    DateTime,
    fadeAnimation,
    MathHelper,
    NotificationService,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent extends AppComponentBase implements OnInit {
    private assetVersion = MathHelper.guid();

    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Output()
    public deleting = new EventEmitter();

    @Output()
    public failed = new EventEmitter();

    public progress = 0;

    public get previewUrl(): string {
        return this.apiUrl.buildUrl(`api/assets/${this.asset.id}/?width=230&height=155&mode=Crop&q=${this.assetVersion}`);
    }

    public get downloadUrl(): string {
        return this.apiUrl.buildUrl(`api/assets/${this.asset.id}/?q=${this.assetVersion}`);
    }

    public get fileType(): string {
        return this.asset.mimeType.split('/')[1];
    }

    public get fileName(): string {
        return this.asset.fileName;
    }

    public get fileIcon(): string {
        return fileIcon(this.asset.mimeType);
    }

    public get fileInfo(): string {
        let result = '';

        if (this.asset != null) {
            if (this.asset.pixelWidth) {
                result = `${this.asset.pixelWidth}x${this.asset.pixelHeight}px, `;
            }

            result += fileSize(this.asset.fileSize);
        }

        return result;
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly assetsService: AssetsService,
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
        super(notifications, users, apps);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.appName()
                .switchMap(app => this.assetsService.uploadFile(app, initFile))
                .subscribe(result => {
                    if (result instanceof AssetCreatedDto) {
                        setTimeout(() => {
                            const me = `subject:${this.authService.user!.id}`;

                            const asset = new AssetDto(
                                this.asset.id,
                                me, me,
                                DateTime.now(),
                                DateTime.now(),
                                result.fileName,
                                result.fileSize,
                                result.mimeType,
                                result.isImage,
                                result.pixelWidth,
                                result.pixelHeight,
                                result.version);

                            this.asset = asset;
                            this.assetVersion = MathHelper.guid();
                            this.progress = 0;
                        }, 2000);
                    } else {
                        this.progress = result;
                    }
                }, error => {
                    this.failed.emit();

                    this.notifyError(error);
                });
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.appName()
                .switchMap(app => this.assetsService.replaceFile(app, this.asset.id, files[0]))
                .subscribe(result => {
                    if (result instanceof AssetReplacedDto) {
                        setTimeout(() => {
                            const me = `subject:${this.authService.user!.id}`;

                            const asset = new AssetDto(
                                this.asset.id,
                                this.asset.createdBy, me,
                                this.asset.created, DateTime.now(),
                                this.asset.fileName,
                                result.fileSize,
                                result.mimeType,
                                result.isImage,
                                result.pixelWidth,
                                result.pixelHeight,
                                result.version);
                            this.asset = asset;
                            this.assetVersion = MathHelper.guid();
                            this.progress = 0;
                        }, 2000);
                    } else {
                        this.progress = result;
                    }
                }, error => {
                    this.progress = 0;

                    this.notifyError(error);
                });
        }
    }
}

function fileSize(b: number) {
    let u = 0, s = 1024;

    while (b >= s || -b >= s) {
        b /= s;
        u++;
    }

    return (u ? b.toFixed(1) + ' ' : b) + ' kMGTPEZY'[u] + 'B';
}

const mimeMapping = {
    'pdf': 'pdf',
    'vnd.openxmlformats-officedocument.wordprocessingml.document': 'docx',
    'vnd.openxmlformats-officedocument.wordprocessingml.template': 'docx',
    'vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'xlsx',
    'vnd.openxmlformats-officedocument.spreadsheetml.template': 'xlsx',
    'vnd.openxmlformats-officedocument.presentationml.presentation': 'pptx',
    'vnd.openxmlformats-officedocument.presentationml.template': 'pptx',
    'vnd.openxmlformats-officedocument.presentationml.slideshow': 'pptx',
    'msword': 'doc',
    'vnd.ms-word': 'doc',
    'vnd.ms-word.document.macroEnabled.12': 'docx',
    'vnd.ms-word.template.macroEnabled.12': 'docx',
    'vnd.ms-excel': 'xls',
    'vnd.ms-excel.sheet.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.template.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.addin.macroEnabled.12': 'xlsx',
    'vnd.ms-excel.sheet.binary.macroEnabled.12': 'xlsx',
    'vnd.ms-powerpoint': 'ppt',
    'vnd.ms-powerpoint.addin.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.presentation.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.template.macroEnabled.12': 'pptx',
    'vnd.ms-powerpoint.slideshow.macroEnabled.12': 'pptx'
};

function fileIcon(mimeType: string) {
    const mimeParts = mimeType.split('/');

    let mimeIcon = 'generic';

    if (mimeParts.length === 2) {
        const mimePrefix = mimeParts[0].toLowerCase();
        const mimeSuffix = mimeParts[1].toLowerCase();

        if (mimePrefix === 'video') {
            mimeIcon = 'video';
        } else {
            mimeIcon = mimeMapping[mimeSuffix] || 'generic';
        }
    }

    return `/images/asset_${mimeIcon}.png`;
}