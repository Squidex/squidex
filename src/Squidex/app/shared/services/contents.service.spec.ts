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
    ScheduleDto,
    Version
} from './../';

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

        let contents: ContentsDto;

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
                    scheduleJob: {
                        status: 'Draft',
                        scheduledBy: 'Scheduler1',
                        dueTime: '2018-12-12T10:10'
                    },
                    isPending: true,
                    version: 11,
                    data: {},
                    dataDraft: {}
                },
                {
                    id: 'id2',
                    status: 'Published',
                    created: '2016-10-12T10:10',
                    createdBy: 'Created2',
                    lastModified: '2017-10-12T10:10',
                    lastModifiedBy: 'LastModifiedBy2',
                    version: 22,
                    data: {},
                    dataDraft: {}
                }
            ]
        });

        expect(contents!).toEqual(
            new ContentsDto(10, [
                new ContentDto('id1', 'Published',
                    DateTime.parseISO_UTC('2016-12-12T10:10'), 'Created1',
                    DateTime.parseISO_UTC('2017-12-12T10:10'), 'LastModifiedBy1',
                    new ScheduleDto('Draft', 'Scheduler1', DateTime.parseISO_UTC('2018-12-12T10:10')),
                    true,
                    {},
                    {},
                    new Version('11')),
                new ContentDto('id2', 'Published',
                    DateTime.parseISO_UTC('2016-10-12T10:10'), 'Created2',
                    DateTime.parseISO_UTC('2017-10-12T10:10'), 'LastModifiedBy2',
                    null,
                    false,
                    {},
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

        let content: ContentDto;

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
            scheduleJob: {
                status: 'Draft',
                scheduledBy: 'Scheduler1',
                dueTime: '2018-12-12T10:10'
            },
            isPending: true,
            data: {},
            dataDraft: {}
        }, {
            headers: {
                etag: '2'
            }
        });

        expect(content!).toEqual(
            new ContentDto('id1', 'Published',
                DateTime.parseISO_UTC('2016-12-12T10:10'), 'Created1',
                DateTime.parseISO_UTC('2017-12-12T10:10'), 'LastModifiedBy1',
                new ScheduleDto('Draft', 'Scheduler1', DateTime.parseISO_UTC('2018-12-12T10:10')),
                true,
                {},
                {},
                new Version('2')));
    }));

    it('should make post request to create content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        let content: ContentDto;

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

        expect(content!).toEqual(
            new ContentDto('id1', 'Published',
                DateTime.parseISO_UTC('2016-12-12T10:10'), 'Created1',
                DateTime.parseISO_UTC('2017-12-12T10:10'), 'LastModifiedBy1',
                null,
                true,
                null,
                {},
                new Version('2')));
    }));

    it('should make get request to get versioned content data',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const response = {};

        let data: any;

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

        contentsService.putContent('my-app', 'my-schema', 'content1', dto, true, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1?asDraft=true');

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