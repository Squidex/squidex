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
    AssetsDto,
    AssetsService,
    DateTime,
    ErrorDto,
    Resource,
    ResourceLinks,
    Version
} from '@app/shared/internal';

describe('AssetsService', () => {
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
                assetResponse(12),
                assetResponse(13)
            ]
        });

        expect(assets!).toEqual(
            new AssetsDto(10, [
                createAsset(12),
                createAsset(13)
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

        req.flush(assetResponse(12));

        expect(asset!).toEqual(createAsset(12));
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

        assetsService.uploadFile('my-app', null!).subscribe(result => {
            asset = <AssetDto>result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(assetResponse(12));

        expect(asset!).toEqual(createAsset(12));
    }));

    it('should return proper error when upload failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        let asset: AssetDto;
        let error: ErrorDto;

        assetsService.uploadFile('my-app', null!).subscribe(result => {
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

        const resource: Resource = {
            _links: {
                upload: { method: 'PUT', href: 'api/apps/my-app/assets/123/content' }
            }
        };

        let asset: AssetDto;

        assetsService.replaceFile('my-app', resource, null!, version).subscribe(result => {
            asset = <AssetDto>result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(assetResponse(123), {
            headers: {
                etag: '1'
            }
        });

        expect(asset!).toEqual(createAsset(123));
    }));

    it('should return proper error when replace failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                upload: { method: 'PUT', href: 'api/apps/my-app/assets/123/content' }
            }
        };

        let asset: AssetDto;
        let error: ErrorDto;

        assetsService.replaceFile('my-app', resource, null!, version).subscribe(result => {
            asset = <AssetDto>result;
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

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: 'api/apps/my-app/assets/123' }
            }
        };

        let asset: AssetDto;

        assetsService.putAsset('my-app', resource, dto, version).subscribe(result => {
            asset = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(assetResponse(123), {
            headers: {
                etag: '1'
            }
        });

        expect(asset!).toEqual(createAsset(123));
    }));

    it('should make delete request to delete asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: 'api/apps/my-app/assets/123' }
            }
        };

        assetsService.deleteAsset('my-app', resource, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    function assetResponse(id: number, suffix = '') {
        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00`,
            createdBy: `creator-${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00`,
            lastModifiedBy: `modifier-${id}`,
            fileName: `My Name${id}${suffix}.png`,
            fileHash: `My Hash${id}${suffix}`,
            fileType: 'png',
            fileSize: id * 2,
            fileVersion: id * 4,
            mimeType: 'image/png',
            isImage: true,
            pixelWidth: id * 3,
            pixelHeight: id * 5,
            slug: `my-name${id}${suffix}.png`,
            tags: ['tag1', 'tag2'],
            version: id,
            _links: {
                update: { method: 'PUT', href: `/assets/${id}` }
            },
            _meta: {
                isDuplicate: 'true'
            }
        };
    }
});

export function createAsset(id: number, tags?: string[], suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/assets/${id}` }
    };

    const meta = {
        isDuplicate: 'true'
    };

    return new AssetDto(links, meta,
        `id${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-12-12T10:10:00`), `creator-${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`), `modifier-${id}`,
        `My Name${id}${suffix}.png`,
        `My Hash${id}${suffix}`,
        'png',
        id * 2,
        id * 4,
        'image/png',
        true,
        id * 3,
        id * 5,
        `my-name${id}${suffix}.png`,
        tags || ['tag1', 'tag2'],
        new Version(`${id}`));
}