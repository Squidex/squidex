/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DropdownMenuComponent, FileDropDirective, HTTP, ModalDirective, ModalPlacementDirective, ProgressBarComponent, TranslatePipe } from '@app/framework';
import { AppsState, AssetsState, AssetUploaderState, ModalModel, Types, Upload } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-uploader',
    styleUrls: ['./asset-uploader.component.scss'],
    templateUrl: './asset-uploader.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        FileDropDirective,
        ModalDirective,
        ModalPlacementDirective,
        ProgressBarComponent,
        TranslatePipe,
    ],
})
export class AssetUploaderComponent {
    private readonly assetsState = inject(AssetsState);

    protected readonly appsState = inject(AppsState);

    protected readonly assetUploader = inject(AssetUploaderState);

    protected modalMenu = new ModalModel();

    protected addFiles(files: ReadonlyArray<HTTP.UploadFile>) {
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

    protected stopUpload(upload: Upload) {
        this.assetUploader.stopUpload(upload);
    }
}
