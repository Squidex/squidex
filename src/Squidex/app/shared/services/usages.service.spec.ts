/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    CallsUsageDto,
    CurrentCallsDto,
    CurrentStorageDto,
    DateTime,
    StorageUsageDto,
    UsagesService
} from './../';

describe('UsagesService', () => {
    let authService: IMock<AuthService>;
    let usagesService: UsagesService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        usagesService = new UsagesService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get calls usages', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/calls/2017-10-12/2017-10-13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
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
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: CallsUsageDto[] | null = null;

        usagesService.getCallsUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(
            [
                new CallsUsageDto(DateTime.parseISO_UTC('2017-10-12'), 10, 130),
                new CallsUsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170)
            ]);

        authService.verifyAll();
    });

    it('should make get request to get month calls', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/calls/month'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: { count: 130 }
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: CurrentCallsDto | null = null;

        usagesService.getMonthCalls('my-app').subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(new CurrentCallsDto(130));

        authService.verifyAll();
    });

    it('should make get request to get storage usages', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/storage/2017-10-12/2017-10-13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
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
                        ]
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: StorageUsageDto[] | null = null;

        usagesService.getStorageUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(
            [
                new StorageUsageDto(DateTime.parseISO_UTC('2017-10-12'), 10, 130),
                new StorageUsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170)
            ]);

        authService.verifyAll();
    });

    it('should make get request to get today storage', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/storage/today'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: { size: 130 }
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: CurrentStorageDto | null = null;

        usagesService.getTodayStorage('my-app').subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(new CurrentStorageDto(130));

        authService.verifyAll();
    });
});