/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, NgZone, OnInit } from '@angular/core';
import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AppComponentBase,
    AppsStoreService,
    AssetDto,
    AssetsService,
    fadeAnimation,
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
    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    public get previewUrl(): Observable<string> {
        return this.appName().map(app => this.apiUrl.buildUrl(`api/assets/${this.asset.id}/?width=230&height=155&mode=Crop`));
    }

    public get fileType(): string {
        let result = '';

        if (this.asset != null) {
            result = this.asset.mimeType.split('/')[1];
        }

        return result;
    }

    public get fileName(): string {
        let result = '';

        if (this.asset != null) {
            result = this.asset.fileName;
        }

        return result;
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
        private readonly apiUrl: ApiUrlConfig,
        private readonly zone: NgZone
    ) {
        super(notifications, users, apps);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.appName()
                .switchMap(app => this.assetsService.uploadFile(app, initFile)).delay(2000)
                .subscribe(result => {
                    this.zone.run(() => {
                        if (result instanceof AssetDto) {
                            this.asset = result;
                        }
                    });
                }, error => {
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