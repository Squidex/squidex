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
    Resource,
    ResourceLinks,
    ScheduleDto,
    Version,
    Versioned
} from '@app/shared/internal';

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

        contentsService.getContents('my-app', 'my-schema', 17, 13, undefined, undefined, ['Draft', 'Published']).subscribe(result => {
            contents = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?$top=17&$skip=13&status=Draft&status=Published');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            total: 10,
            items: [
                contentResponse(12),
                contentResponse(13)
            ],
            statuses: [{
                status: 'Draft', color: 'Gray'
            }]
        });

        expect(contents!).toEqual(
            new ContentsDto([{ status: 'Draft', color: 'Gray' }], 10, [
                createContent(12),
                createContent(13)
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

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
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

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make get request to get versioned content data',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const response = {};

        let data: Versioned<any>;

        contentsService.getVersionData('my-app', 'my-schema', 'content1', version).subscribe(result => {
            data = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/1');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(response);

        expect(data!.payload).toBe(response);
    }));

    it('should make put request to update content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/content/my-app/my-schema/content1?asDraft=true' }
            }
        };

        let content: ContentDto;

        contentsService.putContent('my-app', resource, dto, version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1?asDraft=true');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make patch request to update content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        const resource: Resource = {
            _links: {
                patch: { method: 'PATCH', href: '/api/content/my-app/my-schema/content1' }
            }
        };

        let content: ContentDto;

        contentsService.patchContent('my-app', resource, dto, version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1');

        expect(req.request.method).toEqual('PATCH');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make put request to discard draft',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                ['draft/discard']: { method: 'PUT', href: '/api/content/my-app/my-schema/content1/discard' }
            }
        };

        let content: ContentDto;

        contentsService.discardDraft('my-app', resource, version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/discard');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make put request to propose draft',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const dto = {};

        const resource: Resource = {
            _links: {
                ['draft/propose']: { method: 'PUT', href: '/api/content/my-app/my-schema/content1/status' }
            }
        };

        let content: ContentDto;

        contentsService.proposeDraft('my-app', resource, dto, version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/status');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make put request to publish draft',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                ['draft/publish']: { method: 'PUT', href: '/api/content/my-app/my-schema/content1/status' }
            }
        };

        let content: ContentDto;

        contentsService.publishDraft('my-app', resource, null, version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/status');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make put request to change content status',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                ['status/published']: { method: 'PUT', href: '/api/content/my-app/my-schema/content1/status' }
            }
        };

        let content: ContentDto;

        contentsService.putStatus('my-app', resource, 'published', '2016-12-12T10:10:00', version).subscribe(result => {
            content = result;
        });

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/status');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(contentResponse(12));

        expect(content!).toEqual(createContent(12));
    }));

    it('should make delete request to delete content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/content/my-app/my-schema/content1' }
            }
        };

        contentsService.deleteContent('my-app', resource, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    function contentResponse(id: number) {
        return {
            id: `id${id}`,
            status: `Status${id}`,
            statusColor: 'black',
            created: `${id % 1000 + 2000}-12-12T10:10:00`,
            createdBy: `creator-${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00`,
            lastModifiedBy: `modifier-${id}`,
            scheduleJob: {
                status: 'Draft',
                scheduledBy: `Scheduler${id}`,
                dueTime: `${id % 1000 + 2000}-11-11T10:10:00`
            },
            isPending: true,
            data: {},
            dataDraft: {},
            version: `${id}`,
            _links: {
                update: { method: 'PUT', href: `/contents/id${id}` }
            }
        };
    }
});

export function createContent(id: number, suffix = '') {
    const links: ResourceLinks = {
        update:  { method: 'PUT', href: `/contents/id${id}` }
    };

    return new ContentDto(links,
        `id${id}`,
        `Status${id}${suffix}`,
        'black',
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-12-12T10:10:00`), `creator-${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`), `modifier-${id}`,
        new ScheduleDto('Draft', `Scheduler${id}`, DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`)),
        true,
        {},
        {},
        new Version(`${id}`));
}