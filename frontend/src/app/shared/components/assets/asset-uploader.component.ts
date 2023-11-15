/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { ProgressBarComponent } from '@app/framework';
import { DropdownMenuComponent } from '@app/framework';
import { ModalDirective } from '@app/framework';
import { FileDropDirective } from '@app/framework';
import { AppsState, AssetsState, AssetUploaderState, ModalModel, Types, Upload } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploader',
    styleUrls: ['./asset-uploader.component.scss'],
    templateUrl: './asset-uploader.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        FileDropDirective,
        ModalDirective,
        DropdownMenuComponent,
        NgFor,
        NgSwitch,
        NgSwitchCase,
        NgSwitchDefault,
        ProgressBarComponent,
        AsyncPipe,
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
