/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { inject, TestBed } from '@angular/core/testing';
import { IMock, Mock } from 'typemoq';
import { ApiUrlConfig, DateTime, formatHistoryMessage, HistoryEventDto, HistoryService, UserDto, UsersProviderService } from '@app/shared/internal';

describe('formatHistoryMessage', () => {
    let userProvider: IMock<UsersProviderService>;

    beforeEach(() => {
        userProvider = Mock.ofType<UsersProviderService>();
    });

    it('should provide simple message', () => {
        const message = formatHistoryMessage('message', userProvider.object)();

        expect(message).toEqual('message');
    });

    it('should embed marker', () => {
        const message = formatHistoryMessage('{Name}', userProvider.object)();

        expect(message).toEqual('<span class="marker-ref">Name</span>');
    });

    it('should embed marker ref and escape HTML', () => {
        const message = formatHistoryMessage('{<h1>HTML</h1>}', userProvider.object)();

        expect(message).toEqual('<span class="marker-ref">&lt;h1&gt;HTML&lt;/h1&gt;</span>');
    });

    it('should embed user ref with unknown type', () => {
        const message = formatHistoryMessage('{unknown:User1}', userProvider.object)();

        expect(message).toEqual('<span class="user-ref">User1</span>');
    });

    it('should embed user ref with client', () => {
        const message = formatHistoryMessage('{user:unknown:Client1}', userProvider.object)();

        expect(message).toEqual('<span class="user-ref">Client1-client</span>');
    });

    it('should embed user ref with client ending with client', () => {
        const message = formatHistoryMessage('{user:client:Sample-Client}', userProvider.object)();

        expect(message).toEqual('<span class="user-ref">Sample-Client</span>');
    });

    it('should embed user ref with subject', () => {
        userProvider.setup(x => x.user('1', null))
            .returns(() => signal(new UserDto({ id: '1', displayName: 'User1' } as any)));

        const message = formatHistoryMessage('{user:subject:1}', userProvider.object)();

        expect(message).toEqual('<span class="user-ref">User1</span>');
    });

    it('should embed user ref with id', () => {
        userProvider.setup(x => x.user('1', null))
            .returns(() => signal(new UserDto({ id: '1', displayName: 'User1' } as any)));

        const message = formatHistoryMessage('{user:1}', userProvider.object)();

        expect(message).toEqual('<span class="user-ref">User1</span>');
    });

    it('should return null if user not loaded yet', () => {
        userProvider.setup(x => x.user('1', null))
            .returns(() => signal(undefined));

        const message = formatHistoryMessage('{user:1}', userProvider.object)();

        expect(message).toBeNull();
    });
});

describe('HistoryService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                HistoryService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get history events', inject([HistoryService, HttpTestingController], (historyService: HistoryService, httpMock: HttpTestingController) => {
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

    it('should make get request to get history events for a team', inject([HistoryService, HttpTestingController], (historyService: HistoryService, httpMock: HttpTestingController) => {
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

    function historyResponse() {
        return [
            {
                actor: 'User1',
                created: '2016-12-12T10:10',
                eventId: '1',
                eventType: 'Type 1',
                message: 'Message 1',
                version: 2,
            },
            {
                actor: 'User2',
                created: '2016-12-13T10:10',
                eventId: '2',
                eventType: 'Type 2',
                message: 'Message 2',
                version: 3,
            },
        ];
    }
});

export function createHistory() {
    return [
        new HistoryEventDto({
            actor: 'User1',
            created: DateTime.parseISO('2016-12-12T10:10'),
            eventId: '1',
            eventType: 'Type 1',
            message: 'Message 1',
            version: 2,
        }),
        new HistoryEventDto({
            actor: 'User2',
            created: DateTime.parseISO('2016-12-13T10:10'),
            eventId: '2',
            eventType: 'Type 2',
            message: 'Message 2',
            version: 3,
        }),
    ];
}
