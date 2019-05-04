/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AnalyticsService,
    ApiUrlConfig,
    AssetDto,
    AssetReplacedDto,
    AssetsDto,
    AssetsService,
    DateTime,
    ErrorDto,
    Version,
    Versioned
} from '@app/shared/internal';

describe('AssetDto', () => {
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update tag property and user info when annnoting', () => {
        const update = { fileName: 'New-Name.png' };

        const asset_1 = new AssetDto('1', creator, creator, creation, creation, 'Name.png', 'Hash', 'png', 1, 1, 'image/png', false, false, 1, 1, 'name.png', [], 'url', version);
        const asset_2 = asset_1.annnotate(update, modifier, newVersion, modified);

        expect(asset_2.fileName).toEqual(update.fileName);
        expect(asset_2.tags).toEqual([]);
        expect(asset_2.slug).toEqual(asset_1.slug);
        expect(asset_2.lastModified).toEqual(modified);
        expect(asset_2.lastModifiedBy).toEqual(modifier);
        expect(asset_2.version).toEqual(newVersion);
    });

    it('should update file properties when uploading', () => {
        const update = {
            fileHash: 'Hash New',
            fileSize: 1024,
            fileVersion: 12,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
        };

        const asset_1 = new AssetDto('1', creator, creator, creation, creation, 'Name.png', 'Hash', 'png', 1, 1, 'image/png', false, false, 1, 1, 'name.png', [], 'url', version);
        const asset_2 = asset_1.update(update, modifier, newVersion, modified);

        expect(asset_2.fileHash).toEqual(update.fileHash);
        expect(asset_2.fileSize).toEqual(update.fileSize);
        expect(asset_2.fileVersion).toEqual(update.fileVersion);
        expect(asset_2.mimeType).toEqual(update.mimeType);
        expect(asset_2.isImage).toBeTruthy();
        expect(asset_2.pixelWidth).toEqual(update.pixelWidth);
        expect(asset_2.pixelHeight).toEqual(update.pixelHeight);
        expect(asset_2.lastModified).toEqual(modified);
        expect(asset_2.lastModifiedBy).toEqual(modifier);
        expect(asset_2.version).toEqual(newVersion);
    });
});

describe('AssetsService', () => {
    const now = DateTime.now();
    const user = 'me';
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AssetsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get asset tags',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let tags: any;

        assetsService.getTags('my-app').subscribe(result => {
            tags = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/tags');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            tag1: 1,
            tag2: 4
        });

        expect(tags!).toEqual({
            tag1: 1,
            tag2: 4
        });
    }));

    it('should make get request to get assets',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let assets: AssetsDto;

        assetsService.getAssets('my-app', 17, 13).subscribe(result => {
            assets = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets?$top=17&$skip=13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            total: 10,
            items: [
                {
                    id: 'id1',
                    created: '2016-12-12T10:10',
                    createdBy: 'Created1',
                    lastModified: '2017-12-12T10:10',
                    lastModifiedBy: 'LastModifiedBy1',
                    fileName: 'My Asset1.png',
                    fileHash: 'My Hash1',
                    fileType: 'png',
                    fileSize: 1024,
                    fileVersion: 2000,
                    mimeType: 'image/png',
                    isImage: true,
                    pixelWidth: 1024,
                    pixelHeight: 2048,
                    slug: 'my-asset1.png',
                    tags: undefined,
                    version: 11
                },
                {
                    id: 'id2',
                    created: '2016-10-12T10:10',
                    createdBy: 'Created2',
                    lastModified: '2017-10-12T10:10',
                    lastModifiedBy: 'LastModifiedBy2',
                    fileName: 'My Asset2.png',
                    fileHash: 'My Hash1',
                    fileType: 'png',
                    fileSize: 1024,
                    fileVersion: 2000,
                    mimeType: 'image/png',
                    isImage: true,
                    pixelWidth: 1024,
                    pixelHeight: 2048,
                    slug: 'my-asset2.png',
                    tags: ['tag1', 'tag2'],
                    version: 22
                }
            ]
        });

        expect(assets!).toEqual(
            new AssetsDto(10, [
                new AssetDto(
                    'id1', 'Created1', 'LastModifiedBy1',
                    DateTime.parseISO_UTC('2016-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    'My Asset1.png',
                    'My Hash1',
                    'png',
                    1024,
                    2000,
                    'image/png',
                    false,
                    true,
                    1024,
                    2048,
                    'my-asset1.png',
                    [],
                    'http://service/p/api/assets/id1',
                    new Version('11')),
                new AssetDto('id2', 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    'My Asset2.png',
                    'My Hash1',
                    'png',
                    1024,
                    2000,
                    'image/png',
                    false,
                    true,
                    1024,
                    2048,
                    'my-asset2.png',
                    ['tag1', 'tag2'],
                    'http://service/p/api/assets/id2',
                    new Version('22'))
        ]));
    }));

    it('should make get request to get asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetDto;

        assetsService.getAsset('my-app', '123').subscribe(result => {
            asset = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            fileName: 'My Asset1.png',
            fileHash: 'My Hash1',
            fileType: 'png',
            fileSize: 1024,
            fileVersion: 2000,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048,
            slug: 'my-asset1.png',
            tags: ['tag1', 'tag2']
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(asset!).toEqual(
            new AssetDto(
                'id1', 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                'My Asset1.png',
                'My Hash1',
                'png',
                1024,
                2000,
                'image/png',
                false,
                true,
                1024,
                2048,
                'my-asset1.png',
                ['tag1', 'tag2'],
                'http://service/p/api/assets/id1',
                new Version('2')));
    }));

    it('should append query to find by name',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        assetsService.getAssets('my-app', 17, 13, 'my-query').subscribe();

        const req = httpMock.expectOne(`http://service/p/api/apps/my-app/assets?$filter=contains(fileName,'my-query')&$top=17&$skip=13`);

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append query to find by name and tag',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        assetsService.getAssets('my-app', 17, 13, 'my-query', ['tag1', 'tag2']).subscribe();

        const req = httpMock.expectOne(`http://service/p/api/apps/my-app/assets?$filter=contains(fileName,'my-query') and tags eq 'tag1' and tags eq 'tag2'&$top=17&$skip=13`);

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append ids query to find by ids',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        assetsService.getAssets('my-app', 0, 0, undefined, undefined, ['12', '23']).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets?ids=12,23');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should make post request to create asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetDto;

        assetsService.uploadFile('my-app', null!, user, now).subscribe(result => {
            asset = <AssetDto>result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            fileName: 'My Asset1.png',
            fileHash: 'My Hash1',
            fileType: 'png',
            fileSize: 1024,
            fileVersion: 2,
            mimeType: 'image/png',
            isDuplicate: true,
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048,
            slug: 'my-asset1.png',
            tags: ['tag1', 'tag2']
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(asset!).toEqual(
            new AssetDto(
                'id1',
                user,
                user,
                now,
                now,
                'My Asset1.png',
                'My Hash1',
                'png',
                1024, 2,
                'image/png',
                true,
                true,
                1024,
                2048,
                'my-asset1.png',
                ['tag1', 'tag2'],
                'http://service/p/api/assets/id1',
                new Version('2')));
    }));

    it('should return proper error when upload failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetDto;
        let error: ErrorDto;

        assetsService.uploadFile('my-app', null!, user, now).subscribe(result => {
            asset = <AssetDto>result;
        }, e => {
            error = e;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({}, { status: 413, statusText: 'Payload too large' });

        expect(asset!).toBeUndefined();
        expect(error!).toEqual(new ErrorDto(413, 'Asset is too big.'));
    }));

    it('should make put request to replace asset content',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetReplacedDto;

        assetsService.replaceFile('my-app', '123', null!, version).subscribe(result => {
            asset = (<Versioned<AssetReplacedDto>>result).payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({
            fileHash: 'Hash New',
            fileSize: 1024,
            fileVersion: 12,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
        });

        expect(asset!).toEqual({
            fileHash: 'Hash New',
            fileSize: 1024,
            fileVersion: 12,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
        });
    }));

    it('should return proper error when replace failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetReplacedDto;
        let error: ErrorDto;

        assetsService.replaceFile('my-app', '123', null!, version).subscribe(result => {
            asset = (<Versioned<AssetReplacedDto>>result).payload;
        }, e => {
            error = e;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({}, { status: 413, statusText: 'Payload too large' });

        expect(asset!).toBeUndefined();
        expect(error!).toEqual(new ErrorDto(413, 'Asset is too big.'));
    }));

    it('should make put request to annotate asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        const dto = { fileName: 'New-Name.png' };

        assetsService.putAsset('my-app', '123', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to delete asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        assetsService.deleteAsset('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});