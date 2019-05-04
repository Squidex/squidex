/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { distinctUntilChanged, map, publishReplay, refCount, takeUntil } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    MathHelper,
    State,
    Types
} from '@app/framework';

import { AssetDto, AssetsService } from './../services/assets.service';
import { AuthService } from './../services/auth.service';
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
    uploads: UploadList;
}

export class UploadCanceled { }

type UploadList = ImmutableArray<Upload>;
type UploadResult = AssetDto | number;

@Injectable()
export class AssetUploaderState extends State<Snapshot> {
    public uploads =
        this.changes.pipe(map(x => x.uploads),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly authService: AuthService,
        private readonly dialogs: DialogService
    ) {
        super({ uploads: ImmutableArray.empty() });
    }

    public stopUpload(upload: Upload) {
        upload.cancel.error(new UploadCanceled());

        this.next(s => {
            const uploads = s.uploads.removeBy('id', upload);

            return { ...s, uploads };
        });
    }

    public uploadFile(file: File, target?: AssetsState, now?: DateTime): Observable<UploadResult> {
        const stream = this.assetsService.uploadFile(this.appName, file, this.user, now || DateTime.now());

        return this.upload(stream, MathHelper.guid(), file, asset => {
            if (asset.isDuplicate) {
                this.dialogs.notifyError('Asset has already been uploaded.');
            } else if (target) {
                target.add(asset);
            }

            return asset;
        });
    }

    public uploadAsset(asset: AssetDto, file: File, now?: DateTime): Observable<UploadResult> {
        const stream = this.assetsService.replaceFile(this.appName, asset.id, file, asset.version);

        return this.upload(stream, asset.id, file, ({ version, payload }) => asset.update(payload, this.user, version, now));
    }

    private upload<T>(source: Observable<number | T>, id: string, file: File, complete: ((completion: T) => AssetDto)) {
        let upload = { id, name: file.name, progress: 1, status: 'Running', cancel: new Subject() };

        this.addUpload(upload);

        const stream = source.pipe(takeUntil(upload.cancel),
            map(event => {
                if (Types.isNumber(event)) {
                    return event;
                } else {
                    return complete(event);
                }
            }),
            publishReplay(), refCount());

        stream.subscribe(event => {
            if (Types.isNumber(event)) {
                upload = this.update(upload, { progress: event });
            }
        }, () => {
            upload = this.remove(upload, { status: 'Failed' });
        }, () => {
            upload = this.remove(upload, { status: 'Completed', progress: 100 });
        });

        return stream;
    }

    private remove(upload: Upload, update: Partial<Upload>) {
        upload = this.update(upload, update);

        setTimeout(() => {
            this.next(s => {
                const uploads = s.uploads.removeBy('id', upload);

                return { ...s, uploads };
            });

        }, 10000);

        return upload;
    }

    private update(upload: Upload, update: Partial<Upload>) {
        upload = { ...upload, ...update };

        this.next(s => {
            const uploads = s.uploads.replaceBy('id', upload);

            return { ...s, uploads };
        });

        return upload;
    }

    private addUpload(upload: Upload) {
        this.next(s => {
            const uploads = s.uploads.push(upload);

            return { ...s, uploads };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get user() {
        return this.authService.user!.token;
    }
}