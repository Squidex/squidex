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
    ContentDto,
    ContentsDto,
    ContentsService,
    DateTime,
    Version
} from './../';

describe('ContentDto', () => {
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const dueTime = DateTime.now().addDays(1);
    const version = new Version('1');
    const newVersion = new Version('2');

    it('should update data property and user info when updating', () => {
        const content_1 = new ContentDto('1', 'Published', creator, creator, creation, creation, null, null, null, { data: 1 }, version);
        const content_2 = content_1.update({ data: 2 }, modifier, newVersion, modified);

        expect(content_2.data).toEqual({ data: 2 });
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
        expect(content_2.version).toEqual(newVersion);
    });

    it('should update status property and user info when changing status', () => {
        const content_1 = new ContentDto('1', 'Draft', creator, creator, creation, creation, null, null, null, { data: 1 }, version);
        const content_2 = content_1.changeStatus('Published', null, modifier, newVersion, modified);

        expect(content_2.status).toEqual('Published');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
        expect(content_2.version).toEqual(newVersion);
    });

    it('should update schedules property and user info when changing status with due time', () => {
        const content_1 = new ContentDto('1', 'Draft', creator, creator, creation, creation, null, null, null, { data: 1 }, version);
        const content_2 = content_1.changeStatus('Published', dueTime, modifier, newVersion, modified);

        expect(content_2.status).toEqual('Draft');
        expect(content_2.lastModified).toEqual(modified);
        expect(content_2.lastModifiedBy).toEqual(modifier);
        expect(content_2.scheduledAt).toEqual(dueTime);
        expect(content_2.scheduledBy).toEqual(modifier);
        expect(content_2.scheduledTo).toEqual('Published');
        expect(content_2.version).toEqual(newVersion);
    });

    it('should update data property when setting data', () => {
        const newData = {};

        const content_1 = new ContentDto('1', 'Published', creator, creator, creation, creation, null, null, null, { data: 1 }, version);
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
                    scheduledTo: 'Draft',
                    scheduledBy: 'Scheduler1',
                    scheduledAt: '2018-12-12T10:10',
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
                    'Draft',
                    'Scheduler1',
                    DateTime.parseISO_UTC('2018-12-12T10:10'),
                    {},
                    new Version('11')),
                new ContentDto('id2', 'Published', 'Created2', 'LastModifiedBy2',
                    DateTime.parseISO_UTC('2016-10-12T10:10'),
                    DateTime.parseISO_UTC('2017-10-12T10:10'),
                    null,
                    null,
                    null,
                    {},
                    new Version('22'))
        ]));
    }));

    it('should append query to get request as search',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.getContents('my-app', 'my-schema', 17, 13, 'my-query').subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$search="my-query"&$top=17&$skip=13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append ids to get request with ids',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.getContents('my-app', 'my-schema', 17, 13, undefined, ['id1', 'id2']).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$top=17&$skip=13&ids=id1,id2');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should append query to get request as plain query string',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.getContents('my-app', 'my-schema', 17, 13, '$filter=my-filter').subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$filter=my-filter&$top=17&$skip=13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ total: 10, items: [] });
    }));

    it('should make get request to get content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        let content: ContentDto | null = null;

        contentsService.getContent('my-app', 'my-schema', '1').subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/1');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: 'id1',
            status: 'Published',
            created: '2016-12-12T10:10',
            createdBy: 'Created1',
            lastModified: '2017-12-12T10:10',
            lastModifiedBy: 'LastModifiedBy1',
            scheduledTo: 'Draft',
            scheduledBy: 'Scheduler1',
            scheduledAt: '2018-12-12T10:10',
            data: {}
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(content).toEqual(
            new ContentDto('id1', 'Published', 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                'Draft',
                'Scheduler1',
                DateTime.parseISO_UTC('2018-12-12T10:10'),
                {},
                new Version('2')));
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
            data: {}
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(content).toEqual(
            new ContentDto('id1', 'Published', 'Created1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                null,
                null,
                null,
                {},
                new Version('2')));
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
        expect(req.request.headers.get('If-Match')).toBeNull();

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

    it('should make patch request to update content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        contentsService.patchContent('my-app', 'my-schema', 'content1', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1');

        expect(req.request.method).toEqual('PATCH');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({});
    }));

    it('should make put request to change content status',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        contentsService.changeContentStatus('my-app', 'my-schema', 'content1', 'publish', null, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/publish');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request with due time when status change is scheduled',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dueTime = '2016-12-12T10:10:00';

        contentsService.changeContentStatus('my-app', 'my-schema', 'content1', 'publish', dueTime, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/publish?dueTime=2016-12-12T10:10:00');

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