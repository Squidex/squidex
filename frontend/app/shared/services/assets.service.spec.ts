/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, AssetDto, AssetFolderDto, AssetFoldersDto, AssetsDto, AssetsService, DateTime, encodeQuery, ErrorDto, MathHelper, Resource, ResourceLinks, sanitize, Version } from '@app/shared/internal';

describe('AssetsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                AssetsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
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
                tag2: 4,
            });

            expect(tags!).toEqual({
                tag1: 1,
                tag2: 4,
            });
        }));

    it('should make get request to get assets',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let assets: AssetsDto;

            assetsService.getAssets('my-app', { take: 17, skip: 13 }).subscribe(result => {
                assets = result;
            });

            const query = { take: 17, skip: 13 };

            const req = httpMock.expectOne(`http://service/p/api/apps/my-app/assets?q=${encodeQuery(query)}`);

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                total: 10,
                items: [
                    assetResponse(12),
                    assetResponse(13),
                ],
                folders: [
                    assetFolderResponse(22),
                    assetFolderResponse(23),
                ],
            });

            expect(assets!).toEqual(
                new AssetsDto(10, [
                    createAsset(12),
                    createAsset(13),
                ]));
        }));

    it('should make get request to get asset folders',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let assets: AssetFoldersDto;

            assetsService.getAssetFolders('my-app', 'parent1', 'Path').subscribe(result => {
                assets = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/folders?parentId=parent1&scope=Path');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                total: 10,
                items: [
                    assetFolderResponse(22),
                    assetFolderResponse(23),
                ],
                path: [
                    assetFolderResponse(44),
                ],
            });

            expect(assets!).toEqual(
                new AssetFoldersDto(10, [
                    createAssetFolder(22),
                    createAssetFolder(23),
                ], [
                    createAssetFolder(44),
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

    it('should make get request to get assets by name',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const query = { fullText: 'my-query' };

            assetsService.getAssets('my-app', { take: 17, skip: 13, query }).subscribe();

            const expectedQuery = { filter: { and: [{ path: 'fileName', op: 'contains', value: 'my-query' }] }, take: 17, skip: 13 };

            const req = httpMock.expectOne(`http://service/p/api/apps/my-app/assets?q=${encodeQuery(expectedQuery)}`);

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ total: 10, items: [] });
        }));

    it('should make post request to get assets by name if request limit reached',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const query = { fullText: 'my-query' };

            assetsService.getAssets('my-app', { take: 17, skip: 13, query, maxLength: 5 }).subscribe();

            const expectedQuery = { filter: { and: [{ path: 'fileName', op: 'contains', value: 'my-query' }] }, take: 17, skip: 13 };

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/query');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.body).toEqual({ q: sanitize(expectedQuery) });

            req.flush({ total: 10, items: [] });
        }));

    it('should make get request to get assets by tag',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            assetsService.getAssets('my-app', { take: 17, skip: 13, tags: ['tag1'] }).subscribe();

            const expectedQuery = { filter: { and: [{ path: 'tags', op: 'eq', value: 'tag1' }] }, take: 17, skip: 13 };

            const req = httpMock.expectOne(`http://service/p/api/apps/my-app/assets?q=${encodeQuery(expectedQuery)}`);

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ total: 10, items: [] });
        }));

    it('should make get request to get assets by tag if request limit reached',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            assetsService.getAssets('my-app', { take: 17, skip: 13, tags: ['tag1'], maxLength: 5 }).subscribe();

            const expectedQuery = { filter: { and: [{ path: 'tags', op: 'eq', value: 'tag1' }] }, take: 17, skip: 13 };

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/query');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.body).toEqual({ q: sanitize(expectedQuery) });

            req.flush({ total: 10, items: [] });
        }));

    it('should make get request to get assets by ids',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const ids = ['1', '2'];

            assetsService.getAssets('my-app', { ids }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets?ids=1,2');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ total: 10, items: [] });
        }));

    it('should make post request to create asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let asset: AssetDto;

            assetsService.postAssetFile('my-app', null!).subscribe(result => {
                asset = <AssetDto>result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(assetResponse(12));

            expect(asset!).toEqual(createAsset(12));
        }));

    it('should make post with parent id to create asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let asset: AssetDto;

            assetsService.postAssetFile('my-app', null!, 'parent1').subscribe(result => {
                asset = <AssetDto>result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets?parentId=parent1');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(assetResponse(12));

            expect(asset!).toEqual(createAsset(12));
        }));

    it('should return proper error if upload failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let asset: AssetDto;
            let error: ErrorDto;

            assetsService.postAssetFile('my-app', null!).subscribe(result => {
                asset = <AssetDto>result;
            }, e => {
                error = e;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 413, statusText: 'Payload too large' });

            expect(asset!).toBeUndefined();
            expect(error!).toEqual(new ErrorDto(413, 'i18n:assets.fileTooBig'));
        }));

    it('should make put request to replace asset content',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    upload: { method: 'PUT', href: 'api/apps/my-app/assets/123/content' },
                },
            };

            let asset: AssetDto;

            assetsService.putAssetFile('my-app', resource, null!, version).subscribe(result => {
                asset = <AssetDto>result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(assetResponse(123));

            expect(asset!).toEqual(createAsset(123));
        }));

    it('should return proper error if replacing asset content failed with 413',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    upload: { method: 'PUT', href: 'api/apps/my-app/assets/123/content' },
                },
            };

            let asset: AssetDto;
            let error: ErrorDto;

            assetsService.putAssetFile('my-app', resource, null!, version).subscribe(result => {
                asset = <AssetDto>result;
            }, e => {
                error = e;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({}, { status: 413, statusText: 'Payload too large' });

            expect(asset!).toBeUndefined();
            expect(error!).toEqual(new ErrorDto(413, 'i18n:assets.fileTooBig'));
        }));

    it('should make put request to annotate asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const dto = { fileName: 'New-Name.png' };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: 'api/apps/my-app/assets/123' },
                },
            };

            let asset: AssetDto;

            assetsService.putAsset('my-app', resource, dto, version).subscribe(result => {
                asset = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(assetResponse(123));

            expect(asset!).toEqual(createAsset(123));
        }));

    it('should make delete request to move asset item',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    move: { method: 'DELETE', href: 'api/apps/my-app/assets/123/parent' },
                },
            };

            const dto = { parentId: 'parent1' };

            assetsService.putAssetItemParent('my-app', resource, dto, version).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/parent');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({});
        }));

    it('should make delete request to delete asset item',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: 'api/apps/my-app/assets/123' },
                },
            };

            assetsService.deleteAssetItem('my-app', resource, true, version).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123?checkReferrers=true');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({});
        }));

    it('should make post request to create asset folder',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const dto = { folderName: 'My Folder' };

            let assetFolder: AssetFolderDto;

            assetsService.postAssetFolder('my-app', dto).subscribe(result => {
                assetFolder = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/folders');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(assetFolderResponse(22));

            expect(assetFolder!).toEqual(createAssetFolder(22));
        }));

    it('should make put request to update asset folder',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const dto = { folderName: 'My Folder' };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: 'api/apps/my-app/assets/folders/123' },
                },
            };

            let assetFolder: AssetFolderDto;

            assetsService.putAssetFolder('my-app', resource, dto, version).subscribe(result => {
                assetFolder = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/folders/123');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(assetFolderResponse(22));

            expect(assetFolder!).toEqual(createAssetFolder(22));
        }));

    function assetResponse(id: number, suffix = '', parentId?: string) {
        parentId = parentId || MathHelper.EMPTY_GUID;

        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            createdBy: `creator${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00Z`,
            lastModifiedBy: `modifier${id}`,
            fileName: `My Name${key}.png`,
            fileHash: `My Hash${key}`,
            fileType: 'png',
            fileSize: id * 2,
            fileVersion: id * 4,
            isProtected: true,
            parentId,
            mimeType: 'image/png',
            type: `my-type${key}`,
            metadataText: `my-metadata${key}`,
            metadata: {
                pixelWidth: id * 3,
                pixelHeight: id * 5,
            },
            slug: `my-name${key}.png`,
            tags: [
                'tag1',
                'tag2',
            ],
            version: id,
            _links: {
                update: { method: 'PUT', href: `/assets/${id}` },
            },
            _meta: {
                isDuplicate: 'true',
            },
        };
    }

    function assetFolderResponse(id: number, suffix = '', parentId?: string) {
        parentId = parentId || MathHelper.EMPTY_GUID;

        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            folderName: `My Folder${key}`,
            parentId,
            version: id,
            _links: {
                update: { method: 'PUT', href: `/assets/folders/${id}` },
            },
        };
    }
});

export function createAsset(id: number, tags?: ReadonlyArray<string>, suffix = '', parentId?: string) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/assets/${id}` },
    };

    const key = `${id}${suffix}`;

    const meta = {
        isDuplicate: 'true',
    };

    parentId = parentId || MathHelper.EMPTY_GUID;

    return new AssetDto(links, meta,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`), `creator${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`), `modifier${id}`,
        new Version(key),
        `My Name${key}.png`,
        `My Hash${key}`,
        'png',
        id * 2,
        id * 4,
        true,
        parentId,
        'image/png',
        `my-type${key}`,
        `my-metadata${key}`,
        {
            pixelWidth: id * 3,
            pixelHeight: id * 5,
        },
        `my-name${key}.png`,
        tags || [
            'tag1',
            'tag2',
        ]);
}

export function createAssetFolder(id: number, suffix = '', parentId?: string) {
    parentId = parentId || MathHelper.EMPTY_GUID;

    const key = `${id}${suffix}`;

    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/assets/folders/${id}` },
    };

    return new AssetFolderDto(links, `id${id}`, `My Folder${key}`, parentId, new Version(`${id}`));
}
