/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ErrorDto } from '@app/framework';
import { ApiUrlConfig, ContentDto, ContentsDto, ContentsService, DateTime, Resource, ResourceLinks, ScheduleDto, Version, Versioned } from '@app/shared/internal';
import { BulkResultDto, BulkUpdateDto } from './contents.service';
import { sanitize } from './query';

describe('ContentsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                ContentsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make post request to get contents with json query',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const query = { fullText: 'my-query' };

            let contents: ContentsDto;

            contentsService.getContents('my-app', 'my-schema', { take: 17, skip: 13, query }).subscribe(result => {
                contents = result;
            });

            const expectedQuery = { ...query, take: 17, skip: 13 };

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/query');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-NoSlowTotal')).toBeNull();
            expect(req.request.headers.get('X-NoTotal')).toBeNull();
            expect(req.request.body).toEqual({ q: sanitize(expectedQuery) });

            req.flush({
                total: 10,
                items: [
                    contentResponse(12),
                    contentResponse(13),
                ],
                statuses: [{
                    status: 'Draft', color: 'Gray',
                }],
            });

            expect(contents!).toEqual({
                items: [
                    createContent(12),
                    createContent(13),
                ],
                total: 10,
                statuses: [
                    { status: 'Draft', color: 'Gray' },
                ],
                canCreate: false,
                canCreateAndPublish: false,
            });
        }));

    it('should make post request to get contents with odata filter',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const query = { fullText: '$filter=my-filter' };

            contentsService.getContents('my-app', 'my-schema', { take: 17, skip: 13, query }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/query');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-NoSlowTotal')).toBeNull();
            expect(req.request.headers.get('X-NoTotal')).toBeNull();
            expect(req.request.body).toEqual({ odata: '$filter=my-filter&$top=17&$skip=13' });

            req.flush({ total: 10, items: [] });
        }));

    it('should make post request to get all contents by ids',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const ids = ['1', '2', '3'];

            contentsService.getAllContents('my-app', { ids }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/content/my-app');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-NoSlowTotal')).toBeNull();
            expect(req.request.headers.get('X-NoTotal')).toBeNull();
            expect(req.request.body).toEqual({ ids });

            req.flush({ total: 10, items: [] });
        }));

    it('should make post request to get contents without total',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            contentsService.getContents('my-app', 'my-schema', { noTotal: true, noSlowTotal: true }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/query');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-NoSlowTotal')).toBe('1');
            expect(req.request.headers.get('X-NoTotal')).toBe('1');

            req.flush({ total: 10, items: [] });
        }));

    it('should make post request to get all contents without total',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            contentsService.getAllContents('my-app', { ids: [], noTotal: true, noSlowTotal: true }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/content/my-app');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-NoSlowTotal')).toBe('1');
            expect(req.request.headers.get('X-NoTotal')).toBe('1');

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

            contentsService.postContent('my-app', 'my-schema', dto, true, 'my-id').subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema?publish=true&id=my-id');

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
                    update: { method: 'PUT', href: '/api/content/my-app/my-schema/content1?asDraft=true' },
                },
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
                    patch: { method: 'PATCH', href: '/api/content/my-app/my-schema/content1' },
                },
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

    it('should make post request to create draft',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'draft/create': { method: 'POST', href: '/api/content/my-app/my-schema/content1/draft' },
                },
            };

            let content: ContentDto;

            contentsService.createVersion('my-app', resource, version).subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/draft');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(contentResponse(12));

            expect(content!).toEqual(createContent(12));
        }));

    it('should make delete request to delete draft',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'draft/delete': { method: 'DELETE', href: '/api/content/my-app/my-schema/content1/draft' },
                },
            };

            let content: ContentDto;

            contentsService.deleteVersion('my-app', resource, version).subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/draft');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(contentResponse(12));

            expect(content!).toEqual(createContent(12));
        }));

    it('should make delete request to cancel content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    cancel: { method: 'DELETE', href: '/api/content/my-app/my-schema/content1/status' },
                },
            };

            let content: ContentDto;

            contentsService.cancelStatus('my-app', resource, version).subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/content1/status');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(contentResponse(12));

            expect(content!).toEqual(createContent(12));
        }));

    it('should make post request to for bulk update',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            const dto: BulkUpdateDto = {
                jobs: [{
                    id: '123',
                    type: 'Delete',
                }, {
                    id: '456',
                    type: 'Delete',
                }],
            };

            let results: ReadonlyArray<BulkResultDto>;

            contentsService.bulkUpdate('my-app', 'my-schema', dto).subscribe(result => {
                results = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/bulk');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([{
                contentId: '123',
            }, {
                contentId: '456',
                error: {
                    statusCode: 400,
                    message: 'Invalid',
                },
            }]);

            expect(results!).toEqual([
                new BulkResultDto('123'),
                new BulkResultDto('456', new ErrorDto(400, 'Invalid')),
            ]);
        }));

    function contentResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            status: `Status${id}`,
            statusColor: 'black',
            newStatus: `NewStatus${id}`,
            newStatusColor: 'black',
            created: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            createdBy: `creator${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00Z`,
            lastModifiedBy: `modifier${id}`,
            scheduleJob: {
                status: 'Draft',
                scheduledBy: `Scheduler${id}`,
                color: 'red',
                dueTime: `${id % 1000 + 2000}-11-11T10:10:00Z`,
            },
            data: {},
            schemaName: 'my-schema',
            schemaDisplayName: 'MySchema',
            referenceData: {},
            referenceFields: [],
            version: key,
            _links: {
                update: { method: 'PUT', href: `/contents/id${id}` },
            },
        };
    }
});

export function createContent(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/contents/id${id}` },
    };

    const key = `${id}${suffix}`;

    return new ContentDto(links,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`), `creator${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`), `modifier${id}`,
        new Version(key),
        `Status${key}`,
        'black',
        `NewStatus${key}`,
        'black',
        new ScheduleDto('Draft', `Scheduler${id}`, 'red', DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`)),
        {},
        'my-schema',
        'MySchema',
        {},
        []);
}
