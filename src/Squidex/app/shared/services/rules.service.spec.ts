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
    DateTime,
    Resource,
    ResourceLinks,
    RuleDto,
    RuleElementDto,
    RuleElementPropertyDto,
    RuleEventDto,
    RuleEventsDto,
    RulesDto,
    RulesService,
    Version
} from '@app/shared/internal';

describe('RulesService', () => {
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
                readMore: 'link2',
                properties: [{
                    name: 'property1',
                    editor: 'Editor1',
                    display: 'Display1',
                    description: 'Description1',
                    isRequired: true,
                    isFormattable: false
                }, {
                    name: 'property2',
                    editor: 'Editor2',
                    display: 'Display2',
                    description: 'Description2',
                    isRequired: false,
                    isFormattable: true
                }]
            },
            'action1': {
                display: 'display1',
                description: 'description1',
                iconColor: '#111',
                iconImage: '<svg path="1" />',
                readMore: 'link1',
                properties: []
            }
        });

        const action1 = new RuleElementDto('display1', 'description1', '#111', '<svg path="1" />', null, 'link1', []);

        const action2 = new RuleElementDto('display2', 'description2', '#222', '<svg path="2" />', null, 'link2', [
            new RuleElementPropertyDto('property1', 'Editor1', 'Display1', 'Description1', false, true),
            new RuleElementPropertyDto('property2', 'Editor2', 'Display2', 'Description2', true, false)
        ]);

        expect(actions!).toEqual({
            'action1': action1,
            'action2': action2
        });
    }));

    it('should make get request to get app rules',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        let rules: RulesDto;

        rulesService.getRules('my-app').subscribe(result => {
            rules = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            items: [
                ruleResponse(12),
                ruleResponse(13)
            ]
        });

        expect(rules!).toEqual(
            new RulesDto([
                createRule(12),
                createRule(13)
            ]));
    }));

    it('should make post request to create rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const dto = {
            trigger: {
                param1: 1,
                param2: 2,
                triggerType: 'ContentChanged'
            },
            action: {
                param3: 3,
                param4: 4,
                actionType: 'Webhook'
            }
        };

        let rule: RuleDto;

        rulesService.postRule('my-app', dto).subscribe(result => {
            rule = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(ruleResponse(12), {
            headers: {
                etag: '1'
            }
        });

        expect(rule!).toEqual(createRule(12));
    }));

    it('should make put request to update rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const dto = {
            trigger: {
                param1: 1
            },
            action: {
                param3: 2
            }
        };

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app/rules/123' }
            }
        };

        let rule: RuleDto;

        rulesService.putRule('my-app', resource, dto, version).subscribe(result => {
            rule = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(ruleResponse(123));

        expect(rule!).toEqual(createRule(123));
    }));

    it('should make put request to enable rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                enable: { method: 'PUT', href: '/api/apps/my-app/rules/123/enable' }
            }
        };

        let rule: RuleDto;

        rulesService.enableRule('my-app', resource, version).subscribe(result => {
            rule = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/enable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(ruleResponse(123));

        expect(rule!).toEqual(createRule(123));
    }));

    it('should make put request to disable rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                disable: { method: 'PUT', href: '/api/apps/my-app/rules/123/disable' }
            }
        };

        let rule: RuleDto;

        rulesService.disableRule('my-app', resource, version).subscribe(result => {
            rule = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/disable');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(ruleResponse(123));

        expect(rule!).toEqual(createRule(123));
    }));

    it('should make delete request to delete rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/rules/123' }
            }
        };

        rulesService.deleteRule('my-app', resource, version).subscribe();

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
                ruleEventResponse(1),
                ruleEventResponse(2)
            ]
        });

        expect(rules!).toEqual(
            new RuleEventsDto(20, [
                createRuleEvent(1),
                createRuleEvent(2)
            ]));
    }));

    it('should make put request to enqueue rule event',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app/rules/events/123' }
            }
        };

        rulesService.enqueueEvent('my-app', resource).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make delete request to cancel rule event',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {

        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/rules/events/123' }
            }
        };

        rulesService.cancelEvent('my-app', resource).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    function ruleEventResponse(id: number, suffix = '') {
        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00`,
            eventName: `event${id}${suffix}`,
            nextAttempt: `${id % 1000 + 2000}-11-11T10:10`,
            jobResult: `Failed${id}${suffix}`,
            lastDump: `dump${id}${suffix}`,
            numCalls: id,
            description: `url${id}${suffix}`,
            result: `Failed${id}${suffix}`,
            _links: {
                update: { method: 'PUT', href: `/rules/events/${id}` }
            }
        };
    }

    function ruleResponse(id: number, suffix = '') {
        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10`,
            createdBy: `creator-${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10`,
            lastModifiedBy: `modifier-${id}`,
            isEnabled: id % 2 === 0,
            trigger: {
                param1: 1,
                param2: 2,
                triggerType: `ContentChanged${id}${suffix}`
            },
            action: {
                param3: 3,
                param4: 4,
                actionType: `Webhook${id}${suffix}`
            },
            version: id,
            _links: {
                update: { method: 'PUT', href: `/rules/${id}` }
            }
        };
    }
});

export function createRuleEvent(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/rules/events/${id}` }
    };

    return new RuleEventDto(links, `id${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-12-12T10:10:00`),
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`),
        `event${id}${suffix}`,
        `url${id}${suffix}`,
        `dump${id}${suffix}`,
        `Failed${id}${suffix}`,
        `Failed${id}${suffix}`,
        id);
}

export function createRule(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/rules/${id}` }
    };

    return new RuleDto(links,
        `id${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-12-12T10:10:00`), `creator-${id}`,
        DateTime.parseISO_UTC(`${id % 1000 + 2000}-11-11T10:10:00`), `modifier-${id}`,
        new Version(`${id}`),
        id % 2 === 0,
        {
            param1: 1,
            param2: 2,
            triggerType: `ContentChanged${id}${suffix}`
        },
        `ContentChanged${id}${suffix}`,
        {
            param3: 3,
            param4: 4,
            actionType: `Webhook${id}${suffix}`
        },
        `Webhook${id}${suffix}`);
}