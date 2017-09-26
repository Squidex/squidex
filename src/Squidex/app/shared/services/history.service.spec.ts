/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { DateTime } from 'framework';

import {
    ApiUrlConfig,
    HistoryEventDto,
    HistoryService
} from './../';

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

        let events: HistoryEventDto[] | null = null;

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

        expect(events).toEqual(
            [
                new HistoryEventDto('1', 'User1', 'Message 1', 2, DateTime.parseISO_UTC('2016-12-12T10:10')),
                new HistoryEventDto('2', 'User2', 'Message 2', 3, DateTime.parseISO_UTC('2016-12-13T10:10'))
            ]);
    }));
});