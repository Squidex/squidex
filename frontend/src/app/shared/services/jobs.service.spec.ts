/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, JobDto, JobLogMessageDto, JobsDto, JobsService, Resource } from '@app/shared/internal';
import { ResourceLinkDto, RestoreJobDto, RestoreRequestDto } from '../model';

describe('JobsService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        JobsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get jobs',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            let jobs: JobsDto;
            jobsService.getJobs('my-app').subscribe(result => {
                jobs = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/jobs');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                items: [
                    jobResponse(12),
                    jobResponse(13),
                ],
                _links: {},
            });

            expect(jobs!).toEqual(new JobsDto({
                items: [
                    createJob(12),
                    createJob(13),
                ],
                _links: {},
            }));
        }));

    it('should make get request to get restore',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            let restore: RestoreJobDto;
            jobsService.getRestore().subscribe(result => {
                restore = result!;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                url: 'http://url',
                started: '2017-02-03',
                stopped: '2017-02-04',
                status: 'Failed',
                log: [
                    'log1',
                    'log2',
                ],
            });

            expect(restore!).toEqual(new RestoreJobDto({
                url: 'http://url',
                started: DateTime.parseISO('2017-02-03'),
                stopped: DateTime.parseISO('2017-02-04'),
                status: 'Failed',
                log: [
                    'log1',
                    'log2',
                ],
            }));
        }));

    it('should return null if get restore returns 404',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            let restore: RestoreJobDto | null;
            jobsService.getRestore().subscribe(result => {
                restore = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 404, statusText: '404' });

            expect(restore!).toBeNull();
        }));

    it('should throw error if get restore returns non 404',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            let error: any;

            jobsService.getRestore().subscribe({
                error: e => {
                    error = e;
                },
            });

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({}, { status: 500, statusText: '500' });

            expect(error)!.toBeDefined();
        }));

    it('should make post request to start backup',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            jobsService.postBackup('my-app').subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/backups');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make post request to start restore',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            const dto = new RestoreRequestDto({ url: 'http://url' });

            jobsService.postRestore(dto).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/restore');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to remove job',
        inject([JobsService, HttpTestingController], (jobsService: JobsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/jobs/1' },
                },
            };

            jobsService.deleteJob('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/jobs/1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    function jobResponse(id: number) {
        return {
            id: `id${id}`,
            canDownload: false,
            description: `description${id}`,
            log: [
                {
                    timestamp: buildDate(id, 30),
                    message:  `log1_${id}`,
                },
                {
                    timestamp: buildDate(id, 40),
                    message:  `log2_${id}`,
                },
            ],
            status: id % 2 === 0 ? 'Success' : 'Failed',
            started: buildDate(id, 10),
            stopped: buildDate(id, 20),
            taskName: `task${id}`,
            taskArguments: {
                [`arg${id}`]: '42',
            },
            _links: {
                download: { method: 'GET', href: '/api/jobs/1' },
            },
        };
    }
});

export function createJob(id: number) {
    return new JobDto({
        id: `id${id}`,
        canDownload: false,
        description: `description${id}`,
        log: [
            new JobLogMessageDto({
                timestamp: DateTime.parseISO(buildDate(id, 30)),
                message:  `log1_${id}`,
            }),
            new JobLogMessageDto({
                timestamp: DateTime.parseISO(buildDate(id, 40)),
                message:  `log2_${id}`,
            }),
        ],
        started: DateTime.parseISO(buildDate(id, 10)),
        status: id % 2 === 0 ? 'Success' : 'Failed' as any,
        stopped: DateTime.parseISO(buildDate(id, 20)),
        taskName: `task${id}`,
        taskArguments: {
            [`arg${id}`]: '42',
        },
        _links: {
            download: new ResourceLinkDto({ method: 'GET', href: '/api/jobs/1' }),
        },
    });
}

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}