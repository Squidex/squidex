/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { never, of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, Mock } from 'typemoq';

import {
    AssetDto,
    AssetsService,
    AssetsState,
    AssetUploaderState,
    DialogService,
    ofForever,
    Types
} from '@app/shared/internal';

import { createAsset } from './../services/assets.service.spec';

import { TestValues } from './_test-helpers';

describe('AssetUploaderState', () => {
    const {
        app,
        appsState
    } = TestValues;

    let assetsService: IMock<AssetsService>;
    let dialogs: IMock<DialogService>;
    let assetUploader: AssetUploaderState;

    const asset = createAsset(1);

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        assetsService = Mock.ofType<AssetsService>();
        assetUploader = new AssetUploaderState(appsState.object, assetsService.object, dialogs.object);
    });

    afterEach(() => {
        assetsService.verifyAll();
    });

    it('should create initial state when uploading file', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.postAssetFile(app, file, undefined))
            .returns(() => never()).verifiable();

        assetUploader.uploadFile(file).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(1);
    });

    it('should upload file with folder id from asset state', () => {
        const assetsState = Mock.ofType<AssetsState>();

        assetsState.setup(x => x.parentId)
            .returns(() => 'parent1');

        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.postAssetFile(app, file, 'parent1'))
            .returns(() => never()).verifiable();

        assetUploader.uploadFile(file, assetsState.object).subscribe();

        expect().nothing();
    });

    it('should update progress when uploading file makes progress', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.postAssetFile(app, file, undefined))
            .returns(() => ofForever(10, 20)).verifiable();

        assetUploader.uploadFile(file).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(20);
    });

    it('should update status when uploading file failed', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.postAssetFile(app, file, undefined))
            .returns(() => throwError('Error')).verifiable();

        assetUploader.uploadFile(file).pipe(onErrorResumeNext()).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Failed');
        expect(upload.progress).toBe(1);
    });

    it('should update status when uploading file completes', (cb) => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.postAssetFile(app, file, undefined))
            .returns(() => of(10, 20, asset)).verifiable();

        let uploadedAsset: AssetDto;

        assetUploader.uploadFile(file).subscribe(dto => {
            if (Types.is(dto, AssetDto)) {
                uploadedAsset = dto;
            }

            cb();
        });

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Completed');
        expect(upload.progress).toBe(100);
        expect(uploadedAsset!).toEqual(asset);
    });

    it('should create initial state when uploading asset', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.putAssetFile(app, asset, file, asset.version))
            .returns(() => never()).verifiable();

        assetUploader.uploadAsset(asset, file).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(1);
    });

    it('should update progress when uploading asset makes progress', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.putAssetFile(app, asset, file, asset.version))
            .returns(() => ofForever(10, 20)).verifiable();

        assetUploader.uploadAsset(asset, file).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(20);
    });

    it('should update status when uploading asset failed', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.putAssetFile(app, asset, file, asset.version))
            .returns(() => throwError('Error')).verifiable();

        assetUploader.uploadAsset(asset, file).pipe(onErrorResumeNext()).subscribe();

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Failed');
        expect(upload.progress).toBe(1);
    });

    it('should update status when uploading asset completes', () => {
        const file: File = <any>{ name: 'my-file' };

        let updated = createAsset(1, undefined, '_new');

        assetsService.setup(x => x.putAssetFile(app, asset, file, asset.version))
            .returns(() => of(10, 20, updated)).verifiable();

        let uploadedAsset: AssetDto;

        assetUploader.uploadAsset(asset, file).subscribe(dto => {
            if (Types.is(dto, AssetDto)) {
                uploadedAsset = dto;
            }
        });

        const upload = assetUploader.snapshot.uploads[0];

        expect(upload.status).toBe('Completed');
        expect(upload.progress).toBe(100);
        expect(uploadedAsset!).toEqual(updated);
    });
});