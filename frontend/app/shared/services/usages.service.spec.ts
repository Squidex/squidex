/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    ApiUsageDto,
    ApiUsagesDto,
    CurrentStorageDto,
    DateTime,
    StorageUsageDto,
    UsagesService
} from '@app/shared/internal';

describe('UsagesService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                UsagesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get calls usages',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {

        let usages: ApiUsagesDto;

        usagesService.getCallsUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/calls/2017-10-12/2017-10-13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            allowedCalls: 100,
            totalBytes: 1024,
            totalCalls: 40,
            averageMs: 12.4,
            details: {
                category1: [
                    {
                        date: '2017-10-12',
                        totalBytes: 10,
                        totalCalls: 130,
                        averageMs: 12.3
                    },
                    {
                        date: '2017-10-13',
                        totalBytes: 13,
                        totalCalls: 170,
                        averageMs: 33.3
                    }
                ]
            }
        });

        expect(usages!).toEqual(
            new ApiUsagesDto(100, 1024, 40, 12.4, {
                category1: [
                    new ApiUsageDto(DateTime.parseISO_UTC('2017-10-12'), 10, 130, 12.3),
                    new ApiUsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170, 33.3)
                ]
            })
        );
    }));

    it('should make get request to get storage usages',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {

        let usages: ReadonlyArray<StorageUsageDto>;

        usagesService.getStorageUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/storage/2017-10-12/2017-10-13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                date: '2017-10-12',
                count: 10,
                size: 130
            },
            {
                date: '2017-10-13',
                count: 13,
                size: 170
            }
        ]);

        expect(usages!).toEqual(
            [
                new StorageUsageDto(DateTime.parseISO_UTC('2017-10-12'), 10, 130),
                new StorageUsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170)
            ]);
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

        expect(usages!).toEqual(new CurrentStorageDto(130, 150));
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
});