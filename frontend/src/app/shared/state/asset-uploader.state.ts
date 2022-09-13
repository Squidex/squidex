/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { map, Observable, shareReplay, Subject, takeUntil } from 'rxjs';
import { DialogService, MathHelper, State, Types } from '@app/framework';
import { AssetDto, AssetsService } from './../services/assets.service';
import { AppsState } from './apps.state';
import { AssetsState } from './assets.state';

export interface Upload {
    // Unique id.
    id: string;

    // The name of the asset.
    name: string;

    // The upload subscription.
    cancel: Subject<any>;

    // The progress.
    progress: number;

    // The status.
    status: string;
}

interface Snapshot {
    // The uploads.
    uploads: ReadonlyArray<Upload>;
}

export class UploadCanceled {}

@Injectable()
export class AssetUploaderState extends State<Snapshot> {
    public uploads =
        this.project(x => x.uploads);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly dialogs: DialogService,
    ) {
        super({ uploads: [] }, 'AssetUploader');
    }

    public stopUpload(upload: Upload) {
        upload.cancel.error(new UploadCanceled());

        this.next(s => {
            const uploads = s.uploads.removedBy('id', upload);

            return { ...s, uploads };
        }, 'Stopped');
    }

    public uploadFile(file: File, target?: AssetsState, parentId?: string): Observable<AssetDto | number> {
        const stream = this.assetsService.postAssetFile(this.appName, file, parentId ?? target?.parentId);

        return this.upload(stream, MathHelper.guid(), file.name, asset => {
            if (asset.isDuplicate) {
                this.dialogs.notifyError('i18n:assets.duplicateFile');
            } else if (target) {
                target.addAsset(asset);
            }

            return asset;
        });
    }

    public uploadAsset(asset: AssetDto, file: Blob): Observable<AssetDto | number> {
        const stream = this.assetsService.putAssetFile(this.appName, asset, file, asset.version);

        return this.upload(stream, asset.id, file['name'] || asset.fileName);
    }

    private upload(source: Observable<number | AssetDto>, id: string, name: string, complete?: ((completion: AssetDto) => AssetDto)) {
        let upload = { id, name, progress: 1, status: 'Running', cancel: new Subject() };

        this.addUpload(upload);

        const stream = source.pipe(takeUntil(upload.cancel),
            map(event => {
                if (Types.isNumber(event)) {
                    return event;
                } else if (complete) {
                    return complete(event);
                } else {
                    return event;
                }
            }), shareReplay());

        stream.subscribe({
            next: event => {
                if (Types.isNumber(event)) {
                    upload = this.update(upload, { progress: event });
                }
            },
            error: () => {
                upload = this.remove(upload, { status: 'Failed' });
            },
            complete: () => {
                upload = this.remove(upload, { status: 'Completed', progress: 100 });
            },
        });

        return stream;
    }

    private remove(upload: Upload, update: Partial<Upload>) {
        upload = this.update(upload, update);

        setTimeout(() => {
            this.next(s => {
                const uploads = s.uploads.removedBy('id', upload);

                return { ...s, uploads };
            }, 'Upload Done');
        }, 10000);

        return upload;
    }

    private update(upload: Upload, update: Partial<Upload>) {
        upload = { ...upload, ...update };

        this.next(s => {
            const uploads = s.uploads.replacedBy('id', upload);

            return { ...s, uploads };
        }, 'Updated');

        return upload;
    }

    private addUpload(upload: Upload) {
        this.next(s => {
            const uploads = [upload, ...s.uploads];

            return { ...s, uploads };
        }, 'Upload Started');
    }
}
