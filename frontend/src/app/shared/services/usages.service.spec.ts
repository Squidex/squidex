/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, CallsUsageDto, CallsUsagePerDateDto, CurrentStorageDto, DateTime, StorageUsagePerDateDto, UsagesService } from '@app/shared/internal';

describe('UsagesService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        UsagesService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get calls usages',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: CallsUsageDto;
            usagesService.getCallsUsages('my-app', '2017-10-12', '2017-10-13').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/calls/2017-10-12/2017-10-13');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(callsUsageResponse());

            expect(usages!).toEqual(callsUsageResult());
        }));

    it('should make get request to get calls usages for team',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: CallsUsageDto;
            usagesService.getCallsUsagesForTeam('my-team', '2017-10-12', '2017-10-13').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/usages/calls/2017-10-12/2017-10-13');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(callsUsageResponse());

            expect(usages!).toEqual(callsUsageResult());
        }));

    it('should make get request to get storage usages',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: ReadonlyArray<StorageUsagePerDateDto>;
            usagesService.getStorageUsages('my-app', '2017-10-12', '2017-10-13').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/storage/2017-10-12/2017-10-13');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(storageUsageResponse());

            expect(usages!).toEqual(storageUsageResult());
        }));

    it('should make get request to get storage usages for team',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: ReadonlyArray<StorageUsagePerDateDto>;
            usagesService.getStorageUsagesForTeam('my-team', '2017-10-12', '2017-10-13').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/usages/storage/2017-10-12/2017-10-13');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(storageUsageResponse());

            expect(usages!).toEqual(storageUsageResult());
        }));

    it('should make get request to get today storage',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: CurrentStorageDto;
            usagesService.getTodayStorage('my-app').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/storage/today');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ size: 130, maxAllowed: 150 });

            expect(usages!).toEqual(new CurrentStorageDto({ size: 130, maxAllowed: 150 }));
        }));

    it('should make get request to get today storage for team',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let usages: CurrentStorageDto;

            usagesService.getTodayStorageForTeam('my-team').subscribe(result => {
                usages = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/usages/storage/today');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ size: 130, maxAllowed: 150 });

            expect(usages!).toEqual(new CurrentStorageDto({ size: 130, maxAllowed: 150 }));
        }));

    it('should make get request to get log',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {
            let url: string;

            usagesService.getLog('my-app').subscribe(result => {
                url = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/log');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ downloadUrl: 'download/url' });

            expect(url!).toEqual('download/url');
        }));

        function callsUsageResponse() {
            return {
                allowedBytes: 512,
                allowedCalls: 100,
                averageElapsedMs: 12.4,
                blockingApiCalls: 200,
                monthBytes: 256,
                monthCalls: 5120,
                totalBytes: 1024,
                totalCalls: 40,
                details: {
                    category1: [
                        {
                            date: '2017-10-12',
                            totalBytes: 10,
                            totalCalls: 130,
                            averageElapsedMs: 12.3,
                        },
                        {
                            date: '2017-10-13',
                            totalBytes: 13,
                            totalCalls: 170,
                            averageElapsedMs: 33.3,
                        },
                    ],
                },
            };
        }

        function storageUsageResponse() {
            return [
                {
                    date: '2017-10-12',
                    totalCount: 10,
                    totalSize: 130,
                },
                {
                    date: '2017-10-13',
                    totalCount: 13,
                    totalSize: 170,
                },
            ];
        }
});

export function callsUsageResult() {
    return new CallsUsageDto({
        allowedBytes: 512,
        allowedCalls: 100,
        averageElapsedMs: 12.4,
        blockingApiCalls: 200,
        monthBytes: 256,
        monthCalls: 5120,
        totalBytes: 1024,
        totalCalls: 40,
        details: {
            category1: [
                new CallsUsagePerDateDto({
                    date: DateTime.parseISO('2017-10-12'),
                    totalBytes: 10,
                    totalCalls: 130,
                    averageElapsedMs: 12.3,
                }),
                new CallsUsagePerDateDto({
                    date: DateTime.parseISO('2017-10-13'),
                    totalBytes: 13,
                    totalCalls: 170,
                    averageElapsedMs: 33.3,
                }),
            ],
        },
    });
}

function storageUsageResult() {
    return [
        new StorageUsagePerDateDto({
            date: DateTime.parseISO('2017-10-12'),
            totalCount: 10,
            totalSize: 130,
        }),
        new StorageUsagePerDateDto({
            date: DateTime.parseISO('2017-10-13'),
            totalCount: 13,
            totalSize: 170,
        }),
    ];
}