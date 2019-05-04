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
    AssetReplacedDto,
    AssetsService,
    AssetUploaderState,
    DialogService,
    ofForever,
    Types,
    Version,
    versioned
} from '@app/shared/internal';

import { TestValues } from './_test-helpers';

describe('AssetsState', () => {
    const {
        app,
        appsState,
        authService,
        creator,
        creation,
        modified,
        modifier
    } = TestValues;

    let assetsService: IMock<AssetsService>;
    let dialogs: IMock<DialogService>;
    let assetUploader: AssetUploaderState;

    const asset = new AssetDto('id1',
        creator,
        creator,
        creation,
        creation,
        'my-asset',
        'my-hash',
        'png',
        100,
        1,
        'image/png',
        true,
        true,
        800,
        600,
        'my-slug',
        [],
        'http://url',
        new Version('1'));

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        assetsService = Mock.ofType<AssetsService>();
        assetUploader = new AssetUploaderState(appsState.object, assetsService.object, authService.object, dialogs.object);
    });

    afterEach(() => {
        assetsService.verifyAll();
    });

    it('should create initial state when uploading file', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.uploadFile(app, file, modifier, modified))
            .returns(() => never()).verifiable();

        assetUploader.uploadFile(file, undefined, modified).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(1);
    });

    it('should update progress when uploading file makes progress', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.uploadFile(app, file, modifier, modified))
            .returns(() => ofForever(10, 20)).verifiable();

        assetUploader.uploadFile(file, undefined, modified).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(20);
    });

    it('should update status when uploading file failed', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.uploadFile(app, file, modifier, modified))
            .returns(() => throwError('Error')).verifiable();

        assetUploader.uploadFile(file, undefined, modified).pipe(onErrorResumeNext()).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Failed');
        expect(upload.progress).toBe(1);
    });

    it('should update status when uploading file completes', (cb) => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.uploadFile(app, file, modifier, modified))
            .returns(() => of(10, 20, asset)).verifiable();

        let uploadedAsset: AssetDto;

        assetUploader.uploadFile(file, undefined, modified).subscribe(dto => {
            if (Types.is(dto, AssetDto)) {
                uploadedAsset = dto;
            }

            cb();
        });

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Completed');
        expect(upload.progress).toBe(100);
        expect(uploadedAsset!).toBe(asset);
    });

    it('should create initial state when uploading asset', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.replaceFile(app, asset.id, file, asset.version))
            .returns(() => never()).verifiable();

        assetUploader.uploadAsset(asset, file, modified).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(1);
    });

    it('should update progress when uploading asset makes progress', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.replaceFile(app, asset.id, file, asset.version))
            .returns(() => ofForever(10, 20)).verifiable();

        assetUploader.uploadAsset(asset, file, modified).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Running');
        expect(upload.progress).toBe(20);
    });

    it('should update status when uploading asset failed', () => {
        const file: File = <any>{ name: 'my-file' };

        assetsService.setup(x => x.replaceFile(app, asset.id, file, asset.version))
            .returns(() => throwError('Error')).verifiable();

        assetUploader.uploadAsset(asset, file, modified).pipe(onErrorResumeNext()).subscribe();

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Failed');
        expect(upload.progress).toBe(1);
    });

    it('should update status when uploading asset completes', () => {
        const file: File = <any>{ name: 'my-file' };

        let update: AssetReplacedDto = {
            isImage: true,
            mimeType: 'image/jpeg',
            pixelWidth: 800,
            pixelHeight: 600,
            fileHash: 'my-hash2',
            fileSize: 200,
            fileVersion: 2
        };

        const newAsset = asset.update(update, modifier, new Version('2'), modified);

        assetsService.setup(x => x.replaceFile(app, asset.id, file, asset.version))
            .returns(() => of(10, 20, versioned(new Version('2'), update))).verifiable();

        let uploadedAsset: AssetDto;

        assetUploader.uploadAsset(asset, file, modified).subscribe(dto => {
            if (Types.is(dto, AssetDto)) {
                uploadedAsset = dto;
            }
        });

        const upload = assetUploader.snapshot.uploads.at(0);

        expect(upload.status).toBe('Completed');
        expect(upload.progress).toBe(100);
        expect(uploadedAsset!).toEqual(newAsset);
    });
});