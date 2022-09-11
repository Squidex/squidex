/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, Resource, ResourceLinks, RuleDto, RuleElementDto, RuleElementPropertyDto, RuleEventDto, RuleEventsDto, RulesDto, RulesService, Version } from '@app/shared/internal';
import { RuleCompletions } from '..';
import { SimulatedRuleEventDto, SimulatedRuleEventsDto } from './rules.service';

describe('RulesService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                RulesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
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
                action2: {
                    title: 'title2',
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
                        isFormattable: false,
                    }, {
                        name: 'property2',
                        editor: 'Editor2',
                        display: 'Display2',
                        description: 'Description2',
                        isRequired: false,
                        isFormattable: true,
                        options: [
                            'Yes',
                            'No',
                        ],
                    }],
                },
                action1: {
                    title: 'title1',
                    display: 'display1',
                    description: 'description1',
                    iconColor: '#111',
                    iconImage: '<svg path="1" />',
                    readMore: 'link1',
                    properties: [],
                },
            });

            const action1 = new RuleElementDto('title1', 'display1', 'description1', '#111', '<svg path="1" />', null, 'link1', []);

            const action2 = new RuleElementDto('title2', 'display2', 'description2', '#222', '<svg path="2" />', null, 'link2', [
                new RuleElementPropertyDto('property1', 'Editor1', 'Display1', 'Description1', false, true),
                new RuleElementPropertyDto('property2', 'Editor2', 'Display2', 'Description2', true, false, ['Yes', 'No']),
            ]);

            expect(actions!).toEqual({
                action1,
                action2,
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
                    ruleResponse(13),
                ],
                runningRuleId: '12',
            });

            expect(rules!).toEqual({
                items: [
                    createRule(12),
                    createRule(13),
                ],
                runningRuleId: '12',
                canCancelRun: false,
                canCreate: false,
                canReadEvents: false,
            });
        }));

    it('should make post request to create rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const dto = {
                trigger: {
                    param1: 1,
                    param2: 2,
                    triggerType: 'ContentChanged',
                },
                action: {
                    param3: 3,
                    param4: 4,
                    actionType: 'Webhook',
                },
            };

            let rule: RuleDto;

            rulesService.postRule('my-app', dto).subscribe(result => {
                rule = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(ruleResponse(12));

            expect(rule!).toEqual(createRule(12));
        }));

    it('should make put request to update rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const dto: any = {
                trigger: {
                    param1: 1,
                },
                action: {
                    param3: 2,
                },
            };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/rules/123' },
                },
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

    it('should make delete request to delete rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/rules/123' },
                },
            };

            rulesService.deleteRule('my-app', resource, version).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({});
        }));

    it('should make put request to trigger rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    trigger: { method: 'PUT', href: '/api/apps/my-app/rules/123/trigger' },
                },
            };

            rulesService.triggerRule('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/trigger');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make put request to run rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    run: { method: 'PUT', href: '/api/apps/my-app/rules/123/run' },
                },
            };

            rulesService.runRule('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/run');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make put request to run rule from snapshots',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'run/snapshots': { method: 'PUT', href: '/api/apps/my-app/rules/123/run?fromSnapshots=true' },
                },
            };

            rulesService.runRuleFromSnapshots('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/123/run?fromSnapshots=true');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to cancel run rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            rulesService.runCancel('my-app').subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/run');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make get request to get rule events',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            let rules: RuleEventsDto;

            rulesService.getEvents('my-app', 10, 20, '12').subscribe(result => {
                rules = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events?take=10&skip=20&ruleId=12');

            expect(req.request.method).toEqual('GET');

            req.flush({
                total: 20,
                items: [
                    ruleEventResponse(1),
                    ruleEventResponse(2),
                ],
                _links: {
                    cancel: { method: 'DELETE', href: '/rules/events' },
                },
            });

            expect(rules!).toEqual({
                items: [
                    createRuleEvent(1),
                    createRuleEvent(2),
                ],
                _links: {
                    cancel: { method: 'DELETE', href: '/rules/events' },
                },
                total: 20,
                canCancelAll: false,
            });
        }));

    it('should make get request to get simulated rule events',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            let rules: SimulatedRuleEventsDto;

            rulesService.getSimulatedEvents('my-app', '12').subscribe(result => {
                rules = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/12/simulate');

            expect(req.request.method).toEqual('GET');

            req.flush({
                total: 20,
                items: [
                    simulatedRuleEventResponse(1),
                    simulatedRuleEventResponse(2),
                ],
            });

            expect(rules!).toEqual({
                items: [
                    createSimulatedRuleEvent(1),
                    createSimulatedRuleEvent(2),
                ],
                total: 20,
            });
        }));

    it('should make post request to get simulated rule events with action and trigger',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            let rules: SimulatedRuleEventsDto;

            rulesService.postSimulatedEvents('my-app', {}, {}).subscribe(result => {
                rules = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/simulate');

            expect(req.request.method).toEqual('POST');

            req.flush({
                total: 20,
                items: [
                    simulatedRuleEventResponse(1),
                    simulatedRuleEventResponse(2),
                ],
            });

            expect(rules!).toEqual({
                items: [
                    createSimulatedRuleEvent(1),
                    createSimulatedRuleEvent(2),
                ],
                total: 20,
            });
        }));

    it('should make put request to enqueue rule event',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/rules/events/123' },
                },
            };

            rulesService.enqueueEvent('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events/123');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to cancel all rule events',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    cancel: { method: 'DELETE', href: '/api/apps/my-app/rules/events' },
                },
            };

            rulesService.cancelEvents('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/events');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make get request to get completions',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            let completions: RuleCompletions;

            rulesService.getCompletions('my-app', 'TriggerType').subscribe(result => {
                completions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/rules/completion/TriggerType');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([]);

            expect(completions!).toEqual([]);
        }));

    function ruleResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10`,
            createdBy: `creator${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10`,
            lastModifiedBy: `modifier${id}`,
            name: `rule-name${key}`,
            numSucceeded: id * 3,
            numFailed: id * 4,
            lastExecuted: `${id % 1000 + 2000}-10-10T10:10:00Z`,
            isEnabled: id % 2 === 0,
            trigger: {
                param1: 1,
                param2: 2,
                triggerType: `rule-trigger${key}`,
            },
            action: {
                param3: 3,
                param4: 4,
                actionType: `rule-action${key}`,
            },
            version: id,
            _links: {
                update: { method: 'PUT', href: `/rules/${id}` },
            },
        };
    }

    function ruleEventResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            description: `event-url${key}`,
            eventName: `event-name${key}`,
            jobResult: `Failed${key}`,
            lastDump: `event-dump${key}`,
            nextAttempt: `${id % 1000 + 2000}-11-11T10:10`,
            numCalls: id,
            result: `Failed${key}`,
            _links: {
                update: { method: 'PUT', href: `/rules/events/${id}` },
            },
        };
    }

    function simulatedRuleEventResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            eventId: `id${key}`,
            eventName: `name${key}`,
            event: { value: 'simple' },
            enrichedEvent: { value: 'enriched' },
            actionName: `action-name${key}`,
            actionData: `action-data${key}`,
            error: `error${key}`,
            skipReasons: [`reason${key}`],
            _links: {},
        };
    }
});

export function createRule(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/rules/${id}` },
    };

    const key = `${id}${suffix}`;

    return new RuleDto(links,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`), `creator${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`), `modifier${id}`,
        new Version(key),
        id % 2 === 0,
        {
            param1: 1,
            param2: 2,
            triggerType: `rule-trigger${key}`,
        },
        `rule-trigger${key}`,
        {
            param3: 3,
            param4: 4,
            actionType: `rule-action${key}`,
        },
        `rule-action${key}`,
        `rule-name${key}`,
        id * 3,
        id * 4,
        DateTime.parseISO(`${id % 1000 + 2000}-10-10T10:10:00Z`));
}

export function createRuleEvent(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/rules/events/${id}` },
    };

    const key = `${id}${suffix}`;

    return new RuleEventDto(links, `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`),
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`),
        `event-name${key}`,
        `event-url${key}`,
        `event-dump${key}`,
        `Failed${key}`,
        `Failed${key}`,
        id);
}

export function createSimulatedRuleEvent(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new SimulatedRuleEventDto({},
        `id${key}`,
        `name${key}`,
        { value: 'simple' },
        { value: 'enriched' },
        `action-name${key}`,
        `action-data${key}`,
        `error${key}`,
        [`reason${key}`]);
}
