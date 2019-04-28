/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, Subject, Subscription } from 'rxjs';
import { distinctUntilChanged, map } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    MathHelper,
    State,
    Types
} from '@app/framework';

import { AuthService } from '../services/auth.service';
import { AssetDto, AssetsService } from './../services/assets.service';
import { AppsState } from './apps.state';
import { AssetsState } from './assets.state';

export interface Upload {
    // Unique id.
    id: string;

    // The name of the asset.
    name: string;

    // The upload subscription.
    subscription: Subscription;

    // The progress.
    progress: number;

    // The status.
    status: string;
}

interface Snapshot {
    // The uploads.
    uploads: UploadList;
}

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
        upload.subscription.unsubscribe();

        this.next(s => {
            const uploads = s.uploads.removeBy('id', upload);

            return { ...s, uploads };
        });
    }

    public uploadFile(file: File, target?: AssetsState, now?: DateTime): Observable<UploadResult> {
        const observable = this.assetsService.uploadFile(this.appName, file, this.user, now || DateTime.now());

        let upload: Upload;

        const subject = new Subject<UploadResult>();
        const subscription = observable.subscribe(event => {
            if (Types.isNumber(event)) {
                this.update(upload, { progress: event });
            } else {
                if (event.isDuplicate) {
                    this.dialogs.notifyError('Asset has already been uploaded.');
                }

                if (target) {
                    target.add(event);
                }
            }

            subject.next(event);
        }, error => {
            subject.error(error);

            this.remove(upload, 'failed');
        }, () => {
            subject.complete();

            this.remove(upload, 'completed');
        });

        upload = { id: MathHelper.guid(), name: file.name, progress: 1, subscription, status: 'running' };

        this.addUpload(upload);

        return subject;
    }

    public uploadUpdate(asset: AssetDto, file: File, now?: DateTime): Observable<UploadResult> {
        const observable = this.assetsService.replaceFile(this.appName, asset.id, file, asset.version);

        let upload: Upload;

        const subject = new Subject<UploadResult>();
        const subscription = observable.subscribe(event => {
            if (Types.isNumber(event)) {
                this.update(upload, { progress: event });

                subject.next(event);
            } else {
                subject.next(asset.update(event.payload, this.user, event.version, now));
            }

        }, error => {
            subject.error(error);

            this.remove(upload, 'failed');
        }, () => {
            subject.complete();

            this.remove(upload, 'completed');
        });

        upload = { id: asset.id, name: file.name, progress: 1, subscription, status: 'running' };

        this.addUpload(upload);

        return subject;
    }

    private remove(upload: Upload, status: string) {
        this.update(upload, { status });

        setTimeout(() => {
            this.next(s => {
                const uploads = s.uploads.removeBy('id', upload);

                return { ...s, uploads };
            });

        }, 10000);
    }

    private update(upload: Upload, update: Partial<Upload>) {
        this.next(s => {
            const uploads = s.uploads.replaceBy('id', { ...upload, ...update });

            return { ...s, uploads };
        });
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