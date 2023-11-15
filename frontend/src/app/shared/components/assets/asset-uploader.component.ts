/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DropdownMenuComponent, FileDropDirective, ModalDirective, ProgressBarComponent, TranslatePipe } from '@app/framework';
import { AppsState, AssetsState, AssetUploaderState, ModalModel, Types, Upload } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-asset-uploader',
    styleUrls: ['./asset-uploader.component.scss'],
    templateUrl: './asset-uploader.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        DropdownMenuComponent,
        FileDropDirective,
        ModalDirective,
        NgFor,
        NgIf,
        NgSwitch,
        NgSwitchCase,
        NgSwitchDefault,
        ProgressBarComponent,
        TranslatePipe,
    ],
})
export class AssetUploaderComponent {
    public modalMenu = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly assetUploader: AssetUploaderState,
        public readonly assetsState: AssetsState,
    ) {
    }

    public addFiles(files: ReadonlyArray<File>) {
        for (const file of files) {
            this.assetUploader.uploadFile(file)
                .subscribe({
                    next: assetOrProgress => {
                        if (!Types.isNumber(assetOrProgress)) {
                            this.assetsState.addAsset(assetOrProgress);
                        }
                    },
                });
        }

        this.modalMenu.show();
    }

    public stopUpload(upload: Upload) {
        this.assetUploader.stopUpload(upload);
    }

    public trackByUpload(_index: number, upload: Upload) {
        return upload.id;
    }
}
