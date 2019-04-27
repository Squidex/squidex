/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';

import {
    AssetsState,
    DialogModel,
    fadeAnimation,
    UploadingAsset
} from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploader',
    styleUrls: ['./asset-uploader.component.scss'],
    templateUrl: './asset-uploader.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetUploaderComponent {
    public modalMenu = new DialogModel(true);

    constructor(
        public readonly assets: AssetsState
    ) {
    }

    public addFiles(files: File[]) {
        for (let file of files) {
            this.assets.upload(file).subscribe();
        }

        this.modalMenu.show();
    }

    public stopUpload(upload: UploadingAsset) {
        this.assets.remove(upload);
    }

    public trackByUpload(index: number, upload: UploadingAsset) {
        return upload.id;
    }
}