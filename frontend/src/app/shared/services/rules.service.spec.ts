/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, ContentChangedRuleTriggerDto, DateTime, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicRuleDto, DynamicRulesDto, DynamicUpdateRuleDto, FlowDefinitionDto, FlowExecutionStateDto, ManualRuleTriggerDto, Resource, ResourceLinkDto, RuleElementDto, RuleElementPropertyDto, RuleEventDto, RuleEventsDto, RulesService, ScriptCompletions, SimulatedRuleEventDto, SimulatedRuleEventsDto, VersionTag } from '@app/shared/internal';

describe('RulesService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
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
            rulesService.getSteps().subscribe(result => {
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
                    properties: [
                        {
                            name: 'property1',
                            editor: 'Editor1',
                            display: 'Display1',
                            description: 'Description1',
                            isRequired: true,
                            isFormattable: false,
                        },
                        {
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
                        },
                    ],
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

            expect(actions!).toEqual({
                action2: new RuleElementDto({
                    title: 'title2',
                    display: 'display2',
                    description: 'description2',
                    iconColor: '#222',
                    iconImage: '<svg path="2" />',
                    readMore: 'link2',
                    properties: [
                        new RuleElementPropertyDto({
                            name: 'property1',
                            editor: 'Editor1' as any,
                            display: 'Display1',
                            description: 'Description1',
                            isRequired: true,
                            isFormattable: false,
                        }),
                        new RuleElementPropertyDto({
                            name: 'property2',
                            editor: 'Editor2' as any,
                            display: 'Display2',
                            description: 'Description2',
                            isRequired: false,
                            isFormattable: true,
                            options: [
                                'Yes',
                                'No',
                            ],
                        }),
                    ],
                }),
                action1: new RuleElementDto({
                    title: 'title1',
                    display: 'display1',
                    description: 'description1',
                    iconColor: '#111',
                    iconImage: '<svg path="1" />',
                    readMore: 'link1',
                    properties: [],
                }),
            });
        }));

    it('should make get request to get app rules',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            let rules: DynamicRulesDto;
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
                _links: {},
            });

            expect(rules!).toEqual(new DynamicRulesDto({
                items: [
                    createRule(12),
                    createRule(13),
                ],
                runningRuleId: '12',
                _links: {},
            }));
        }));

    it('should make post request to create rule',
        inject([RulesService, HttpTestingController], (rulesService: RulesService, httpMock: HttpTestingController) => {
            const dto = new DynamicCreateRuleDto({
                trigger: new ManualRuleTriggerDto(),
                flow: new DynamicFlowDefinitionDto({
                    steps: {},
                    initialStep: 'NONE',
                }),
            });

            let rule: DynamicRuleDto;
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
            const dto = new DynamicUpdateRuleDto({
                trigger: new ManualRuleTriggerDto(),
                action: {
                    param3: 3,
                    param4: 4,
                    actionType: 'Webhook',
                },
            });

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/rules/123' },
                },
            };

            let rule: DynamicRuleDto;
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

            expect(rules!).toEqual(new RuleEventsDto({
                total: 20,
                items: [
                    createRuleEvent(1),
                    createRuleEvent(2),
                ],
                _links: {
                    cancel: new ResourceLinkDto({ method: 'DELETE', href: '/rules/events' }),
                },
            }));
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
                _links: {},
            });

            expect(rules!).toEqual(new SimulatedRuleEventsDto({
                total: 20,
                items: [
                    createSimulatedRuleEvent(1),
                    createSimulatedRuleEvent(2),
                ],
                _links: {},
            }));
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
                _links: {},
            });

            expect(rules!).toEqual(new SimulatedRuleEventsDto({
                total: 20,
                items: [
                    createSimulatedRuleEvent(1),
                    createSimulatedRuleEvent(2),
                ],
                _links: {},
            }));
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
            let completions: ScriptCompletions;
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
            created: buildDate(id, 10),
            createdBy: `creator${id}`,
            isEnabled: id % 2 === 0,
            lastModified: buildDate(id, 20),
            lastModifiedBy: `modifier${id}`,
            name: `rule-name${key}`,
            numFailed: id * 4,
            numSucceeded: id * 3,
            trigger: {
                triggerType: 'ContentChanged',
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
            flowState: {
                completed: buildDate(id, 20),
                context: {},
                created:buildDate(id, 10),
                definition: {
                    steps: {},
                    initialStep: '0',
                },
                nextStepId: '1',
                status: `Failed${key}` as any,
                steps: {},
            },
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
            flowState: {
                completed: buildDate(id, 20),
                context: {},
                created: buildDate(id, 10),
                definition: {
                    steps: {},
                    initialStep: '0',
                },
                nextStepId: '1',
                status: `Failed${key}` as any,
                steps: {},
            },
            skipReasons: [`reason${key}` as any],
            uniqueId: `unique-id${key}`,
        };
    }
});

export function createRule(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new DynamicRuleDto({
        id: `id${id}`,
        created: DateTime.parseISO(buildDate(id, 10)),
        createdBy: `creator${id}`,
        flow: new DynamicFlowDefinitionDto(),
        isEnabled: id % 2 === 0,
        lastModified: DateTime.parseISO(buildDate(id, 20)),
        lastModifiedBy: `modifier${id}`,
        name: `rule-name${key}`,
        numFailed: id * 4,
        numSucceeded: id * 3,
        trigger: new ContentChangedRuleTriggerDto(),
        action: {
            param3: 3,
            param4: 4,
            actionType: `rule-action${key}`,
        } as any,
        version: id,
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/rules/${id}` }),
        },
    });
}

export function createRuleEvent(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new RuleEventDto({
        id: `id${id}`,
        flowState: new FlowExecutionStateDto({
            completed: DateTime.parseISO(buildDate(id, 20)),
            context: {},
            created: DateTime.parseISO(buildDate(id, 10)),
            definition: new FlowDefinitionDto({
                steps: {},
                initialStep: '0',
            }),
            nextStepId: '1',
            status: `Failed${key}` as any,
            steps: {},
        }),
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/rules/events/${id}` }),
        },
    });
}

export function createSimulatedRuleEvent(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new SimulatedRuleEventDto({
        eventId: `id${key}`,
        eventName: `name${key}`,
        event: { value: 'simple' },
        flowState: new FlowExecutionStateDto({
            completed: DateTime.parseISO(buildDate(id, 20)),
            context: {},
            created: DateTime.parseISO(buildDate(id, 10)),
            definition: new FlowDefinitionDto({
                steps: {},
                initialStep: '0',
            }),
            nextStepId: '1',
            status: `Failed${key}` as any,
            steps: {},
        }),
        skipReasons: [`reason${key}` as any],
        uniqueId: `unique-id${key}`,
    });
}

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}
