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
    CallsUsageDto,
    CurrentCallsDto,
    CurrentStorageDto,
    DateTime,
    StorageUsageDto,
    UsagesService
} from './../';

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

        let usages: { [category: string]: CallsUsageDto[] };

        usagesService.getCallsUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/calls/2017-10-12/2017-10-13');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            category1: [
                {
                    date: '2017-10-12',
                    count: 10,
                    averageMs: 130
                },
                {
                    date: '2017-10-13',
                    count: 13,
                    averageMs: 170
                }
            ]
        });

        expect(usages!).toEqual({
            category1: [
                new CallsUsageDto(DateTime.parseISO_UTC('2017-10-12'), 10, 130),
                new CallsUsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170)
            ]
        });
    }));

    it('should make get request to get month calls',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {

        let usages: CurrentCallsDto;

        usagesService.getMonthCalls('my-app').subscribe(result => {
            usages = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/usages/calls/month');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ count: 130, maxAllowed: 150 });

        expect(usages!).toEqual(new CurrentCallsDto(130, 150));
    }));

    it('should make get request to get storage usages',
        inject([UsagesService, HttpTestingController], (usagesService: UsagesService, httpMock: HttpTestingController) => {

        let usages: StorageUsageDto[];

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
});