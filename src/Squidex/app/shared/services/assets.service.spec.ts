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
    UpdateAssetDto,
    Version,
    Versioned
} from './../';

describe('AssetDto', () => {
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update name property and user info when renaming', () => {
        const asset_1 = new AssetDto('1', creator, creator, creation, creation, 'name.png', 'png', 1, 1, 'image/png', false, 1, 1, 'url', version);
        const asset_2 = asset_1.rename('new-name.png', modifier, newVersion, modified);

        expect(asset_2.fileName).toEqual('new-name.png');
        expect(asset_2.lastModified).toEqual(modified);
        expect(asset_2.lastModifiedBy).toEqual(modifier);
        expect(asset_2.version).toEqual(newVersion);
    });

    it('should update file properties when uploading', () => {
        const update = new AssetReplacedDto(2, 2, 'image/jpeg', true, 2, 2);

        const asset_1 = new AssetDto('1', creator, creator, creation, creation, 'name.png', 'png', 1, 1, 'image/png', false, 1, 1, 'url', version);
        const asset_2 = asset_1.update(update, modifier, newVersion, modified);

        expect(asset_2.fileSize).toEqual(2);
        expect(asset_2.fileVersion).toEqual(2);
        expect(asset_2.mimeType).toEqual('image/jpeg');
        expect(asset_2.isImage).toBeTruthy();
        expect(asset_2.pixelWidth).toEqual(2);
        expect(asset_2.pixelHeight).toEqual(2);
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
                    fileName: 'my-asset1.png',
                    fileType: 'png',
                    fileSize: 1024,
                    fileVersion: 2000,
                    mimeType: 'image/png',
                    isImage: true,
                    pixelWidth: 1024,
                    pixelHeight: 2048,
                    version: 11
                },
                {
                    id: 'id2',
                    created: '2016-10-12T10:10',
                    createdBy: 'Created2',
                    lastModified: '2017-10-12T10:10',
                    lastModifiedBy: 'LastModifiedBy2',
                    fileName: 'my-asset2.png',
                    fileType: 'png',
                    fileSize: 1024,
                    fileVersion: 2000,
                    mimeType: 'image/png',
                    isImage: true,
                    pixelWidth: 1024,
                    pixelHeight: 2048,
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
                    'my-asset1.png',
                    'png',
                    1024,
                    2000,
                    'image/png',
                    true,
                    1024,
                    2048,
                    'http://service/p/api/assets/id1',
                    new Version('11')),
                new AssetDto('id2', 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    'my-asset2.png',
                    'png',
                    1024,
                    2000,
                    'image/png',
                    true,
                    1024,
                    2048,
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
            fileName: 'my-asset1.png',
            fileType: 'png',
            fileSize: 1024,
            fileVersion: 2000,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
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
                'my-asset1.png',
                'png',
                1024,
                2000,
                'image/png',
                true,
                1024,
                2048,
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

    it('should append ids query to find by ids',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        assetsService.getAssets('my-app', 0, 0, undefined, ['12', '23']).subscribe();

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
            fileName: 'my-asset1.png',
            fileType: 'png',
            fileSize: 1024,
            fileVersion: 2,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
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
                'my-asset1.png',
                'png',
                1024, 2,
                'image/png',
                true,
                1024,
                2048,
                'http://service/p/api/assets/id1',
                new Version('2')));
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
            fileSize: 1024,
            fileVersion: 2,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: 1024,
            pixelHeight: 2048
        });

        expect(asset!).toEqual(
            new AssetReplacedDto(
                1024, 2,
                'image/png',
                true,
                1024,
                2048));
    }));

    it('should make put request to update asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        const dto = new UpdateAssetDto('My-Asset.pdf');

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