/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AnalyticsService,
    ApiUrlConfig,
    ContentDto,
    ContentsDto,
    ContentsService,
    DateTime,
    LocalCacheService,
    Version
} from './../';

describe('ContentDto', () => {
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');

    it('should update data property and user info when updating', () => {
        const content_1 = new ContentDto('1', 'Published', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.update({ data: 2 }, modifier, modified);

        expect(content_2.data).toEqual({ data: 2 });
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
    });

    it('should update status property and user info when publishing', () => {
        const content_1 = new ContentDto('1', 'Draft', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.publish(modifier, modified);

        expect(content_2.status).toEqual('Published');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
    });

    it('should update status property and user info when unpublishing', () => {
        const content_1 = new ContentDto('1', 'Published', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.unpublish(modifier, modified);

        expect(content_2.status).toEqual('Draft');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
    });

    it('should update status property and user info when archiving', () => {
        const content_1 = new ContentDto('1', 'Draft', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.archive(modifier, modified);

        expect(content_2.status).toEqual('Archived');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
    });

    it('should update status property and user info when restoring', () => {
        const content_1 = new ContentDto('1', 'Archived', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.restore(modifier, modified);

        expect(content_2.status).toEqual('Draft');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
    });

    it('should update data property when setting data', () => {
        const newData = {};

        const content_1 = new ContentDto('1', 'Published', creator, creator, creation, creation, { data: 1 }, version);
        const content_2 = content_1.setData(newData);

        expect(content_2.data).toBe(newData);
    });
});

describe('ContentsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                ContentsService,
                LocalCacheService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get contents',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, undefined, undefined, true).subscribe(result => {
            contents = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$top=17&$skip=13&archived=true');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            total: 10,
            items: [
                {
                    id: 'id1',
                    status: 'Published',
                    created: '2016-12-12T10:10',
                    createdBy: 'Created1',
                    lastModified: '2017-12-12T10:10',
                    lastModifiedBy: 'LastModifiedBy1',
                    version: 11,
                    data: {}
                },
                {
                    id: 'id2',
                    status: 'Published',
                    created: '2016-10-12T10:10',
                    createdBy: 'Created2',
                    lastModified: '2017-10-12T10:10',
                    lastModifiedBy: 'LastModifiedBy2',
                    version: 22,
                    data: {}
                }
            ]
        });

        expect(contents).toEqual(
            new ContentsDto(10, [
                new ContentDto('id1', 'Published', 'Created1', 'LastModifiedBy1',
                    DateTime.parseISO_UTC('2016-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    {},
                    new Version('11')),
                new ContentDto('id2', 'Published', 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    {},
                    new Version('22'))
        ]));
    }));

    it('should append query to get request as search',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, 'my-query').subscribe(result => {
            contents = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$search="my-query"&$top=17&$skip=13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append ids to get request with ids',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, undefined, ['id1', 'id2']).subscribe(result => {
            contents = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$top=17&$skip=13&ids=id1,id2');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append query to get request as plain query string',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let contents: ContentsDto | null = null;

        contentsService.getContents('my-app', 'my-schema', 17, 13, '$filter=my-filter').subscribe(result => {
            contents = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$filter=my-filter&$top=17&$skip=13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should make get request to get content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let content: ContentDto | null = null;

        contentsService.getContent('my-app', 'my-schema', '1', version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/1');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({
            id: 'id1',
            status: 'Published',
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            version: 11,
            data: {}
        });

        expect(content).toEqual(
            new ContentDto('id1', 'Published', 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                {},
                new Version('11')));
    }));

    it('should provide entry from cache if not found',
        inject([LocalCacheService, ContentsService, HttpTestingController], (localCache: LocalCacheService, contentsService: ContentsService, httpMock: HttpTestingController) => {

        const cached = {};

        localCache.set('content.1', cached, 10000);

        let content: ContentDto | null = null;

        contentsService.getContent('my-app', 'my-schema', '1', version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/1');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({}, { status: 404, statusText: '404' });

        expect(content).toBe(cached);
    }));

    it('should make post request to create content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        let content: ContentDto | null = null;

        contentsService.postContent('my-app', 'my-schema', dto, true).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?publish=true');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            status: 'Published',
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            version: 11,
            data: {}
        });

        expect(content).toEqual(
            new ContentDto('id1', 'Published', 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                {},
                new Version('11')));
    }));

    it('should make get request to get versioned content data',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const response = {};

        let data: any | null = null;

        contentsService.getVersionData('my-app', 'my-schema', 'content1', version).subscribe(result => {
            data = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/1');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(response);

        expect(data).toBe(response);
    }));

    it('should make put request to update content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        contentsService.putContent('my-app', 'my-schema', 'content1', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to publish content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.publishContent('my-app', 'my-schema', 'content1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/publish');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request to unpublish content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.unpublishContent('my-app', 'my-schema', 'content1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/unpublish');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request to archive content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.archiveContent('my-app', 'my-schema', 'content1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/archive');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request to restore content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.restoreContent('my-app', 'my-schema', 'content1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/restore');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to delete content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.deleteContent('my-app', 'my-schema', 'content1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));
});