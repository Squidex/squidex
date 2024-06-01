/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { ApiUrlConfig, DateTime, formatHistoryMessage, HistoryEventDto, HistoryService, UsersProviderService, Version } from '@app/shared/internal';

describe('formatHistoryMessage', () => {
    let userProvider: IMock<UsersProviderService>;

    beforeEach(() => {
        userProvider = Mock.ofType<UsersProviderService>();
    });

    it('should provide simple message', async () => {
        const message = await firstValueFrom(formatHistoryMessage('message', userProvider.object));

        expect(message).toEqual('message');
    });

    it('should embed marker', async () => {
        const message = await firstValueFrom(formatHistoryMessage('{Name}', userProvider.object));

        expect(message).toEqual('<span class="marker-ref">Name</span>');
    });

    it('should embed marker ref and escape HTML', async () => {
        const message = await firstValueFrom(formatHistoryMessage('{<h1>HTML</h1>}', userProvider.object));

        expect(message).toEqual('<span class="marker-ref">&lt;h1&gt;HTML&lt;/h1&gt;</span>');
    });

    it('should embed user ref with unknown type', async () => {
        const message = await firstValueFrom(formatHistoryMessage('{unknown:User1}', userProvider.object));

        expect(message).toEqual('<span class="user-ref">User1</span>');
    });

    it('should embed user ref with client', async () => {
        const message = await firstValueFrom(formatHistoryMessage('{user:unknown:Client1}', userProvider.object));

        expect(message).toEqual('<span class="user-ref">Client1-client</span>');
    });

    it('should embed user ref with client ending with client', async () => {
        const message = await firstValueFrom(formatHistoryMessage('{user:client:Sample-Client}', userProvider.object));

        expect(message).toEqual('<span class="user-ref">Sample-Client</span>');
    });

    it('should embed user ref with subject', async () => {
        userProvider.setup(x => x.getUser('1', null))
            .returns(() => of({ id: '1', displayName: 'User1' }));

        const message = await firstValueFrom(formatHistoryMessage('{user:subject:1}', userProvider.object));

        expect(message).toEqual('<span class="user-ref">User1</span>');
    });

    it('should embed user ref with id', async () => {
        userProvider.setup(x => x.getUser('1', null))
            .returns(() => of({ id: '1', displayName: 'User1' }));

        const message = await firstValueFrom(formatHistoryMessage('{user:1}', userProvider.object));

        expect(message).toEqual('<span class="user-ref">User1</span>');
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
