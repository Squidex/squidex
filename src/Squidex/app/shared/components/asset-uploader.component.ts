/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';

import {
    AssetsState,
    AssetUploaderState,
    DialogModel,
    fadeAnimation,
    Upload
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
        public readonly assetUploader: AssetUploaderState,
        public readonly assetsState: AssetsState
    ) {
    }

    public addFiles(files: File[]) {
        for (let file of files) {
            this.assetUploader.uploadFile(file, this.assetsState);
        }

        this.modalMenu.show();
    }

    public stopUpload(upload: Upload) {
        this.assetUploader.stopUpload(upload);
    }

    public trackByUpload(index: number, upload: Upload) {
        return upload.id;
    }
}