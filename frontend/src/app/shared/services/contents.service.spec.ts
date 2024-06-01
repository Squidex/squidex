/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ErrorDto } from '@app/framework';
import { ApiUrlConfig, ContentDto, ContentsDto, ContentsService, DateTime, Resource, ResourceLinks, ScheduleDto, Version, Versioned } from '@app/shared/internal';
import { BulkResultDto, BulkUpdateDto } from './contents.service';
import { sanitize } from './query';

describe('ContentsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        ContentsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    const tests = [
        {
            name: 'json query',
            query: { take: 17, skip: 13, query: { fullText: 'my-query' } },
            requestBody: { q: sanitize({ fullText: 'my-query', take: 17, skip: 13 }) },
            requestString: `q=${JSON.stringify(sanitize({ fullText: 'my-query', take: 17, skip: 13 }))}`,
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'odata filter',
            query: { take: 17, skip: 13, query: { fullText: '$filter=my-filter' } },
            requestBody: { odata: '$filter=my-filter&$top=17&$skip=13' },
            requestString: '$filter=my-filter&$top=17&$skip=13',
            noSlowTotal: null,
            noTotal: null,
        },
        {
            name: 'json query without total',
            query: { take: 17, skip: 13, query: { fullText: 'my-query' }, noTotal: true, noSlowTotal: true },
            requestBody: { q: sanitize({ fullText: 'my-query', take: 17, skip: 13 }) },
            requestString: `q=${JSON.stringify(sanitize({ fullText: 'my-query', take: 17, skip: 13 }))}`,
            noSlowTotal: '1',
            noTotal: '1',
        },
    ];

    tests.forEach(x => {
        it(`should make post request to get contents using ${x.name}`,
            inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
                let contents: ContentsDto;

                contentsService.getContents('my-app', 'my-schema', x.query).subscribe(result => {
                    contents = result;
                });

                const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/query');

                expect(req.request.method).toEqual('POST');
                expect(req.request.headers.get('If-Match')).toBeNull();
                expect(req.request.headers.get('X-NoSlowTotal')).toEqual(x.noSlowTotal);
                expect(req.request.headers.get('X-NoTotal')).toEqual(x.noTotal);
                expect(req.request.body).toEqual({ ...x.requestBody });

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
    });

    tests.forEach(x => {
        it(`should make post request to get all contents using ${x.name}`,
            inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
                const ids = ['1', '2', '3'];

                contentsService.getAllContents('my-app', { ids }, x.query).subscribe();

                const req = httpMock.expectOne('http://service/p/api/content/my-app');

                expect(req.request.method).toEqual('POST');
                expect(req.request.headers.get('If-Match')).toBeNull();
                expect(req.request.headers.get('X-NoSlowTotal')).toEqual(x.noSlowTotal);
                expect(req.request.headers.get('X-NoTotal')).toEqual(x.noTotal);
                expect(req.request.body).toEqual({ ids, ...x.requestBody });

                req.flush({ total: 10, items: [] });
            }));
        });

    tests.forEach(x => {
        it(`should make get request to get references with using ${x.name}`,
            inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
                contentsService.getContentReferences('my-app', 'my-schema', '42', x.query).subscribe();

                const req = httpMock.expectOne(`http://service/p/api/content/my-app/my-schema/42/references?${x.requestString}`);

                expect(req.request.method).toEqual('GET');
                expect(req.request.headers.get('If-Match')).toBeNull();
                expect(req.request.headers.get('X-NoSlowTotal')).toEqual(x.noSlowTotal);
                expect(req.request.headers.get('X-NoTotal')).toEqual(x.noTotal);

                req.flush({ total: 10, items: [] });
            }));
    });

    tests.forEach(x => {
        it(`should make get request to get referencing with using ${x.name}`,
            inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
                contentsService.getContentReferencing('my-app', 'my-schema', '42', x.query).subscribe();

                const req = httpMock.expectOne(`http://service/p/api/content/my-app/my-schema/42/referencing?${x.requestString}`);

                expect(req.request.method).toEqual('GET');
                expect(req.request.headers.get('If-Match')).toBeNull();
                expect(req.request.headers.get('X-NoSlowTotal')).toEqual(x.noSlowTotal);
                expect(req.request.headers.get('X-NoTotal')).toEqual(x.noTotal);

                req.flush({ total: 10, items: [] });
            }));
    });

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

    it('should make get request to get raw content',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            let content: any;

            contentsService.getRawContent('my-app', 'my-schema', '1').subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/1');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-Flatten')).toBeNull();
            expect(req.request.headers.get('X-Languages')).toBeNull();

            req.flush({ id: '1' });

            expect(content!).toEqual({ id: '1' });
        }));

    it('should make get request to get raw content with language',
        inject([ContentsService, HttpTestingController], (contentsService: ContentsService, httpMock: HttpTestingController) => {
            let content: any;

            contentsService.getRawContent('my-app', 'my-schema', '1', 'en').subscribe(result => {
                content = result;
            });

            const req = httpMock.expectOne('http://service/p/api/content/my-app/my-schema/1');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();
            expect(req.request.headers.get('X-Flatten')).toEqual('1');
            expect(req.request.headers.get('X-Languages')).toEqual('en');

            req.flush({ id: '1' });

            expect(content!).toEqual({ id: '1' });
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
            newStatus: `StatusNew${id}`,
            newStatusColor: 'black',
            created: buildDate(id, 10),
            createdBy: `creator${id}`,
            lastModified: buildDate(id, 20),
            lastModifiedBy: `modifier${id}`,
            scheduleJob: {
                status: 'Draft',
                scheduledBy: `Scheduler${id}`,
                color: 'red',
                dueTime: buildDate(id, 30),
            },
            isDeleted: false,
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
        DateTime.parseISO(buildDate(id, 10)), `creator${id}`,
        DateTime.parseISO(buildDate(id, 20)), `modifier${id}`,
        new Version(key),
        `Status${key}`,
        'black',
        `StatusNew${key}`,
        'black',
        new ScheduleDto('Draft', `Scheduler${id}`, 'red', DateTime.parseISO(buildDate(id, 30))),
        false,
        {},
        'my-schema',
        'MySchema',
        {},
        []);
}

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}
