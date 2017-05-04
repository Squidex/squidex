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
    DateTime,
    MonthlyCallsDto,
    UsageDto,
    UsagesService
} from './../';

describe('UsagesService', () => {
    let authService: IMock<AuthService>;
    let usagesService: UsagesService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        usagesService = new UsagesService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get usages', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/2017-10-12/2017-10-13'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            date: '2017-10-12',
                            count: 1,
                            averageMs: 130
                        }, {
                            date: '2017-10-13',
                            count: 13,
                            averageMs: 170
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: UsageDto[] | null = null;

        usagesService.getUsages('my-app', DateTime.parseISO_UTC('2017-10-12'), DateTime.parseISO_UTC('2017-10-13')).subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(
            [
                new UsageDto(DateTime.parseISO_UTC('2017-10-12'), 1, 130),
                new UsageDto(DateTime.parseISO_UTC('2017-10-13'), 13, 170)
            ]);

        authService.verifyAll();
    });

    it('should make get request to get monthly calls', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/usages/monthly'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: { count: 130 }
                    })
                )
            ))
            .verifiable(Times.once());

        let usages: MonthlyCallsDto | null = null;

        usagesService.getMonthlyCalls('my-app').subscribe(result => {
            usages = result;
        }).unsubscribe();

        expect(usages).toEqual(new MonthlyCallsDto(130));

        authService.verifyAll();
    });
});