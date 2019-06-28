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
    Version,
    Versioned,
    WorkflowDto,
    WorkflowPayload,
    WorkflowsService
} from '@app/shared/internal';

describe('WorkflowsService', () => {

    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                WorkflowsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make a get request to get app workflows',
        inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {

        let workflow: Versioned<WorkflowPayload>;

        workflowsService.getWorkflow('my-app').subscribe(result => {
            workflow = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(workflowsResponse('Draft'),
            {
                headers: {
                    etag: '2'
                }
            });

        expect(workflow!).toEqual({ payload: createWorkflow('Draft'), version: new Version('2') });
    }));

    it('should make a put request to assign a workflow',
        inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {

        const dto = createWorkflow('Draft');

        let workflow: Versioned<WorkflowPayload>;

        workflowsService.putWorkflow('my-app', dto, dto.workflow.serialize(), version).subscribe(result => {
            workflow = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(workflowsResponse('Draft'), {
            headers: {
                etag: '2'
            }
        });

        expect(workflow!).toEqual({ payload: createWorkflow('Draft'), version: new Version('2') });
    }));

    function workflowsResponse(name: string) {
        return {
            workflow: {
                steps: {
                    [`${name}1`]: {
                        transitions: {
                            [`${name}2`]: {
                                expression: 'Expression1', role: 'Role1'
                            }
                        },
                        color: `${name}1`, noUpdate: true
                    },
                    [`${name}2`]: {
                        transitions: {
                            [`${name}1`]: {
                                expression: 'Expression2', role: 'Role2'
                            }
                        },
                        color: `${name}2`, noUpdate: true
                    }
                },
                initial: `${name}1`,
                _links: {
                    update: { method: 'PUT', href: '/api/workflows' }
                }
            },
            _links: {},
            canCreate: true
        };
    }
});

export function createWorkflow(name: string): WorkflowPayload {
    return {
        workflow: new WorkflowDto({
            update: { method: 'PUT', href: '/api/workflows' }
        },
        `${name}1`,
        [
            { name: `${name}1`, color: `${name}1`, noUpdate: true, isLocked: false },
            { name: `${name}2`, color: `${name}2`, noUpdate: true, isLocked: false }
        ],
        [
            { from: `${name}1`, to: `${name}2`, expression: 'Expression1', role: 'Role1' },
            { from: `${name}2`, to: `${name}1`, expression: 'Expression2', role: 'Role2' }
        ]),
        _links: {}
    };
}

describe('Workflow', () => {
    it('should create empty workflow', () => {
        const workflow = new WorkflowDto();

        expect(workflow.initial);
    });

    it('should add step to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}, color: '#00ff00' }
            },
            initial: '1'
        });
    });

    it('should override settings if step already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', noUpdate: true })
                .setStep('1', { color: 'red' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}, color: 'red', noUpdate: true }
            },
            initial: '1'
        });
    });

    it('should return same workflow if step to update is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true });

        const updated = workflow.setStep('1', { color: 'red' });

        expect(updated).toBe(workflow);
    });

    it('should sort steps case invariant', () => {
        const workflow =
            new WorkflowDto()
                .setStep('Z')
                .setStep('a');

        expect(workflow.steps).toEqual([
            { name: 'a' },
            { name: 'Z' }
        ]);
    });

    it('should return same workflow if step to remove is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true });

        const updated = workflow.removeStep('1');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to remove not found', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1');

        const updated = workflow.removeStep('3');

        expect(updated).toBe(workflow);
    });

    it('should remove step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setStep('3', { color: '#0000ff' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('1', '3', { expression: '1 === 3' })
                .setTransition('2', '3', { expression: '2 === 3' })
                .removeStep('1');

        expect(workflow.serialize()).toEqual({
            steps: {
                '2': {
                    transitions: {
                        '3': { expression: '2 === 3' }
                    },
                    color: '#ff0000'
                },
                '3': { transitions: {}, color: '#0000ff' }
            },
            initial: '2'
        });
    });

    it('should make first non-locked step the initial step if initial removed', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2', { isLocked: true })
                .setStep('3')
                .removeStep('1');

        expect(workflow.serialize()).toEqual({
            steps: {
                '2': { transitions: {}, isLocked: true },
                '3': { transitions: {} }
            },
            initial: '3'
        });
    });

    it('should unset initial step if initial removed', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .removeStep('1');

        expect(workflow.serialize()).toEqual({ steps: {}, initial: undefined });
    });

    it('should rename step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setStep('3', { color: '#0000ff' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .setTransition('2', '3', { expression: '2 === 3' })
                .renameStep('1', 'a');

        expect(workflow.serialize()).toEqual({
            steps: {
                'a': {
                    transitions: {
                        '2': { expression: '1 === 2' }
                    },
                    color: '#00ff00'
                },
                '2': {
                    transitions: {
                        'a': { expression: '2 === 1' },
                        '3': { expression: '2 === 3' }
                    },
                    color: '#ff0000'
                },
                '3': { transitions: {}, color: '#0000ff' }
            },
            initial: 'a'
        });
    });

    it('should add transitions to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': {
                    transitions: {
                        '2': { expression: '1 === 2' }
                    }
                },
                '2': {
                    transitions: {
                        '1': { expression: '2 === 1' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should remove transition from workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .removeTransition('1', '2');

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}},
                '2': {
                    transitions: {
                        '1': { expression: '2 === 1' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should override settings if transition already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('2', '1', { expression: '2 === 1', role: 'Role' })
                .setTransition('2', '1', { expression: '2 !== 1' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {} },
                '2': {
                    transitions: {
                        '1': { expression: '2 !== 1', role: 'Role' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should return same workflow if transition to update not found by from step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.setTransition('3', '2', { role: 'Role' });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition to update not found by to step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.setTransition('1', '3', { role: 'Role' });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition to remove not', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.removeTransition('1', '3');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to make initial is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2', { color: '#00ff00', isLocked: true });

        const updated = workflow.setInitial('2');

        expect(updated).toBe(workflow);
    });

    it('should set initial step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setInitial('2');

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {} },
                '2': { transitions: {} }
            },
            initial: '2'
        });
    });

});