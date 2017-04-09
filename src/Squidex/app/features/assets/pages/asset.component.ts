/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AssetDto,
    AssetsService,
    NotificationService,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html'
})
export class AssetComponent extends AppComponentBase implements OnInit {
    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

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
        private readonly assetsService: AssetsService
    ) {
        super(notifications, users, apps);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.appName()
                .switchMap(app => this.assetsService.uploadFile(app, initFile))
                .subscribe(result => {
                    if (result instanceof AssetDto) {
                        this.asset = result;
                    }
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