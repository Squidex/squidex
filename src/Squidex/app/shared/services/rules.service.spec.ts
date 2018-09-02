/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AnalyticsService,
    ApiUrlConfig,
    CreateRuleDto,
    DateTime,
    RuleDto,
    RuleElementDto,
    RuleEventDto,
    RuleEventsDto,
    RulesService,
    UpdateRuleDto,
    Version
} from './../';

describe('RulesService', () => {
    const now = DateTime.now();
    const user = 'me';
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                RulesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get actions',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        let actions: { [ name: string ]: RuleElementDto };

        rulesService.getActions().subscribe(result => {
            actions = result;
        });

        const req = httpMock.expectOne('http://service/p/api/rules/actions');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            'action2': {
                display: 'display2',
                description: 'description2',
                iconColor: '#222',
                iconImage: '<svg path="2" />',
                readMore: 'link2'
            },
            'action1': {
                display: 'display1',
                description: 'description1',
                iconColor: '#111',
                iconImage: '<svg path="1" />',
                readMore: 'link1'
            }
        });

        expect(actions!).toEqual({
            'action1': new RuleElementDto('display1', 'description1', '#111', '<svg path="1" />', 'link1'),
            'action2': new RuleElementDto('display2', 'description2', '#222', '<svg path="2" />', 'link2')
        });
    }));

    it('should make get request to get triggers',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        let triggers: { [ name: string ]: RuleElementDto };

        rulesService.getTriggers().subscribe(result => {
            triggers = result;
        });

        const req = httpMock.expectOne('http://service/p/api/rules/triggers');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            'trigger2': {
                display: 'display2',
                description: 'description2',
                iconColor: '#222',
                iconImage: '<svg path="2" />',
                readMore: 'link2'
            },
            'trigger1': {
                display: 'display1',
                description: 'description1',
                iconColor: '#111',
                iconImage: '<svg path="1" />',
                readMore: 'link1'
            }
        });

        expect(triggers!).toEqual({
            'trigger1': new RuleElementDto('display1', 'description1', '#111', '<svg path="1" />', 'link1'),
            'trigger2': new RuleElementDto('display2', 'description2', '#222', '<svg path="2" />', 'link2')
        });
    }));

    it('should make get request to get app rules',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        let rules: RuleDto[];

        rulesService.getRules('my-app').subscribe(result => {
            rules = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: 'id1',
                created: '2016-12-12T10:10',
                createdBy: 'CreatedBy1',
                lastModified: '2017-12-12T10:10',
                lastModifiedBy: 'LastModifiedBy1',
                url: 'http://squidex.io/hook',
                version: '1',
                trigger: {
                    param1: 1,
                    param2: 2,
                    triggerType: 'ContentChanged'
                },
                action: {
                    param3: 3,
                    param4: 4,
                    actionType: 'Webhook'
                },
                isEnabled: true
            }
        ]);

        expect(rules!).toEqual(
            [
                new RuleDto('id1', 'CreatedBy1', 'LastModifiedBy1',
                    DateTime.parseISO_UTC('2016-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    version,
                    true,
                    {
                        param1: 1,
                        param2: 2,
                        triggerType: 'ContentChanged'
                    },
                    'ContentChanged',
                    {
                        param3: 3,
                        param4: 4,
                        actionType: 'Webhook'
                    },
                    'Webhook')
            ]);
    }));

    it('should make post request to create rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const dto = new CreateRuleDto({
            param1: 1,
            param2: 2,
            triggerType: 'ContentChanged'
        }, {
            param3: 3,
            param4: 4,
            actionType: 'Webhook'
        });

        let rule: RuleDto;

        rulesService.postRule('my-app', dto, user, now).subscribe(result => {
            rule = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ id: 'id1', sharedSecret: 'token1', schemaId: 'schema1' }, {
            headers: {
                etag: '1'
            }
        });

        expect(rule!).toEqual(
            new RuleDto('id1', user, user, now, now,
                version,
                true,
                {
                    param1: 1,
                    param2: 2,
                    triggerType: 'ContentChanged'
                },
                'ContentChanged',
                {
                    param3: 3,
                    param4: 4,
                    actionType: 'Webhook'
                },
                'Webhook'));
    }));

    it('should make put request to update rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const dto = new UpdateRuleDto({ param1: 1 }, { param2: 2 });

        rulesService.putRule('my-app', '123', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request to enable rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        rulesService.enableRule('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/enable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make put request to disable rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        rulesService.disableRule('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/disable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to delete rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        rulesService.deleteRule('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make get request to get app rule events',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        let rules: RuleEventsDto;

        rulesService.getEvents('my-app', 10, 20).subscribe(result => {
            rules = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events?take=10&skip=20');

        expect(req.request.method).toEqual('GET');

        req.flush({
            total: 20,
            items: [
                {
                    id: 'id1',
                    created: '2017-12-12T10:10',
                    eventName: 'event1',
                    nextAttempt: '2017-12-12T12:10',
                    jobResult: 'Failed',
                    lastDump: 'dump1',
                    numCalls: 1,
                    description: 'url1',
                    result: 'Failed'
                },
                {
                    id: 'id2',
                    created: '2017-12-13T10:10',
                    eventName: 'event2',
                    nextAttempt: '2017-12-13T12:10',
                    jobResult: 'Failed',
                    lastDump: 'dump2',
                    numCalls: 2,
                    description: 'url2',
                    result: 'Failed'
                }
            ]
        });

        expect(rules!).toEqual(
            new RuleEventsDto(20, [
                new RuleEventDto('id1',
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T12:10'),
                    'event1', 'url1', 'dump1', 'Failed', 'Failed', 1),
                new RuleEventDto('id2',
                    DateTime.parseISO_UTC('2017-12-13T10:10'),
                    DateTime.parseISO_UTC('2017-12-13T12:10'),
                    'event2', 'url2', 'dump2', 'Failed', 'Failed', 2)
            ]));
    }));

    it('should make put request to enqueue rule event',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        rulesService.enqueueEvent('my-app', '123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});