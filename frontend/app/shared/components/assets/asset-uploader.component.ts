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
    fadeAnimation,
    ModalModel,
    Upload
} from '@app/shared/internal';

import { AppsState } from '@app/shared/internal';

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
    public modalMenu = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly assetUploader: AssetUploaderState,
        public readonly assetsState: AssetsState
    ) {
    }

    public addFiles(files: ReadonlyArray<File>) {
        for (const file of files) {
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