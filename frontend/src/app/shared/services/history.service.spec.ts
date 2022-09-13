/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, HistoryEventDto, HistoryService, Version } from '@app/shared/internal';

describe('HistoryService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                HistoryService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
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

            req.flush(historyResponse());

            expect(events!).toEqual(createHistory());
        }));

    it('should make get request to get history events for a team',
        inject([HistoryService, HttpTestingController], (historyService: HistoryService, httpMock: HttpTestingController) => {
            let events: ReadonlyArray<HistoryEventDto>;

            historyService.getHistoryForTeam('my-team', 'settings.contributors').subscribe(result => {
                events = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/history?channel=settings.contributors');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(historyResponse());

            expect(events!).toEqual(createHistory());
        }));
});

export function createHistory() {
    return [
        new HistoryEventDto('1', 'User1', 'Type 1', 'Message 1', DateTime.parseISO('2016-12-12T10:10Z'), new Version('2')),
        new HistoryEventDto('2', 'User2', 'Type 2', 'Message 2', DateTime.parseISO('2016-12-13T10:10Z'), new Version('3')),
    ];
}

function historyResponse() {
    return [
        {
            actor: 'User1',
            eventId: '1',
            eventType: 'Type 1',
            message: 'Message 1',
            version: 2,
            created: '2016-12-12T10:10',
        },
        {
            actor: 'User2',
            eventId: '2',
            eventType: 'Type 2',
            message: 'Message 2',
            version: 3,
            created: '2016-12-13T10:10',
        },
    ];
}
