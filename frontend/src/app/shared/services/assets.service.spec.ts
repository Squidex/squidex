/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, AssetDto, AssetFolderDto, AssetFoldersDto, AssetsDto, AssetsService, DateTime, ErrorDto, MathHelper, Resource, ResourceLinks, sanitize, ScriptCompletions, Version } from '@app/shared/internal';

describe('AssetsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        AssetsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    const tests = [
        {
            name: 'basic query',
            query: { take: 17, skip: 13 },
            requestBody: { q: sanitize({ take: 17, skip: 13 }), parentId: undefined },
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'basic query without total',
            query: { take: 17, skip: 13, noTotal: true, noSlowTotal: true },
            requestBody: { q: sanitize({ take: 17, skip: 13 }), parentId: undefined },
            noSlowTotal: '1',
            noTotal: '1',
        },
        {
            name: 'query by parent',
            query: { take: 17, skip: 13, parentId: '1' },
            requestBody: { q: sanitize({ take: 17, skip: 13 }), parentId: '1' },
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'query by name',
            query: { take: 17, skip: 13, query: { fullText: 'my-query' } },
            requestBody: { q: sanitize({ filter: { and: [{ path: 'fileName', op: 'contains', value: 'my-query' }] }, take: 17, skip: 13 }), parentId: undefined },
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'query by tag',
            query: { take: 17, skip: 13, tags: ['tag1']  },
            requestBody: { q: sanitize({ filter: { and: [{ path: 'tags', op: 'eq', value: 'tag1' }] }, take: 17, skip: 13 }), parentId: undefined },
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'query by ids',
            query: { ids: ['1', '2']  },
            requestBody: { ids: ['1', '2'] },
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'query by ref',
            query: { ref: '1' },
            requestBody: { q: sanitize({ filter: { or: [{ path: 'id', op: 'eq', value: '1' }, { path: 'slug', op: 'eq', value: '1' }] }, take: 1 }) },
            noSlowTotal: null,
            noTotal: '1',
        },
    ];

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

    it('should make put request to rename asset tag',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const dto = { tagName: 'new-name' };

            let tags: any;

            assetsService.putTag('my-app', 'old-name', dto).subscribe(result => {
                tags = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/tags/old-name');

            expect(req.request.body).toEqual(dto);
            expect(req.request.method).toEqual('PUT');
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

    tests.forEach(x => {
        it(`should make post request to get assets with ${x.name}`,
            inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
                let assets: AssetsDto;

                assetsService.getAssets('my-app', x.query).subscribe(result => {
                    assets = result;
                });

                const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/query');

                expect(req.request.method).toEqual('POST');
                expect(req.request.headers.get('If-Match')).toBeNull();
                expect(req.request.headers.get('X-NoSlowTotal')).toEqual(x.noSlowTotal);
                expect(req.request.headers.get('X-NoTotal')).toEqual(x.noTotal);
                expect(req.request.body).toEqual(x.requestBody);

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

                expect(assets!).toEqual({
                    items: [
                        createAsset(12),
                        createAsset(13),
                    ],
                    total: 10,
                    canCreate: false,
                    canRenameTag: false,
                });
            }));
    });

    it('should make get request to get asset folders',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let assetFolders: AssetFoldersDto;

            assetsService.getAssetFolders('my-app', 'parent1', 'Path').subscribe(result => {
                assetFolders = result;
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

            expect(assetFolders!).toEqual({
                items: [
                    createAssetFolder(22),
                    createAssetFolder(23),
                ],
                path: [
                    createAssetFolder(44),
                ],
                canCreate: false,
            });
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
            let error: ErrorDto;

            assetsService.postAssetFile('my-app', null!).subscribe({
                error: e => {
                    error = e;
                },
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 413, statusText: 'Payload too large' });

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

            let error: ErrorDto;

            assetsService.putAssetFile('my-app', resource, null!, version).subscribe({
                error: e => {
                    error = e;
                },
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/content');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({}, { status: 413, statusText: 'Payload too large' });

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

    it('should make put request to move asset',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    move: { method: 'PUT', href: 'api/apps/my-app/assets/123/parent' },
                },
            };

            let asset: AssetDto;

            assetsService.putAssetParent('my-app', resource, { parentId: 'parent1' }, version).subscribe(result => {
                asset = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/123/parent');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(assetResponse(123));

            expect(asset!).toEqual(createAsset(123));
        }));

    it('should make put request to move asset folder',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    move: { method: 'PUT', href: 'api/apps/my-app/assets/folders/123/parent' },
                },
            };

            let assetFolder: AssetFolderDto;

            assetsService.putAssetFolderParent('my-app', resource, { parentId: 'parent1' }, version).subscribe(result => {
                assetFolder = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/folders/123/parent');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(assetFolderResponse(123));

            expect(assetFolder!).toEqual(createAssetFolder(123));
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

    it('should make get request to get completions',
        inject([AssetsService, HttpTestingController], (assetsService: AssetsService, httpMock: HttpTestingController) => {
            let completions: ScriptCompletions;

            assetsService.getCompletions('my-app').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/completion');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    function assetResponse(id: number, suffix = '', parentId?: string) {
        parentId = parentId || MathHelper.EMPTY_GUID;

        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: buildDate(id, 10),
            createdBy: `creator${id}`,
            lastModified: buildDate(id, 20),
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
        DateTime.parseISO(buildDate(id, 10)), `creator${id}`,
        DateTime.parseISO(buildDate(id, 20)), `modifier${id}`,
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

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}
