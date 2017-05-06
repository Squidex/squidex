/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { DateTime } from 'framework';

import {
    ApiUrlConfig,
    AuthService,
    HistoryEventDto,
    HistoryService
} from './../';

describe('HistoryService', () => {
    let authService: IMock<AuthService>;
    let languageService: HistoryService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        languageService = new HistoryService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get history events', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/history?channel=settings.contributors'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
                            {
                                actor: 'User1',
                                eventId: '1',
                                message: 'Message 1',
                                created: '2016-12-12T10:10'
                            },
                            {
                                actor: 'User2',
                                eventId: '2',
                                message: 'Message 2',
                                created: '2016-12-13T10:10'
                            }
                        ]
                    })
                )
            ))
            .verifiable(Times.once());

        let events: HistoryEventDto[] | null = null;

        languageService.getHistory('my-app', 'settings.contributors').subscribe(result => {
            events = result;
        }).unsubscribe();

        expect(events).toEqual(
            [
                new HistoryEventDto('1', 'User1', 'Message 1', DateTime.parseISO_UTC('2016-12-12T10:10')),
                new HistoryEventDto('2', 'User2', 'Message 2', DateTime.parseISO_UTC('2016-12-13T10:10'))
            ]);

        authService.verifyAll();
    });
});