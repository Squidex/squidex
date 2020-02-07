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
    DateTime,
    HistoryEventDto,
    HistoryService,
    Version
} from '@app/shared/internal';

describe('HistoryService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                HistoryService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get history events',
        inject([HistoryService, HttpTestingController], (historyService: HistoryService, httpMock: HttpTestingController) => {

        let events: ReadonlyArray<HistoryEventDto>;

        historyService.getHistory('my-app', 'settings.contributors').subscribe(result => {
            events = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/history?channel=settings.contributors');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                actor: 'User1',
                eventId: '1',
                message: 'Message 1',
                version: 2,
                created: '2016-12-12T10:10'
            },
            {
                actor: 'User2',
                eventId: '2',
                message: 'Message 2',
                version: 3,
                created: '2016-12-13T10:10'
            }
        ]);

        expect(events!).toEqual(
            [
                new HistoryEventDto('1', 'User1', 'Message 1', DateTime.parseISO_UTC('2016-12-12T10:10'), new Version('2')),
                new HistoryEventDto('2', 'User2', 'Message 2', DateTime.parseISO_UTC('2016-12-13T10:10'), new Version('3'))
            ]);
    }));
});