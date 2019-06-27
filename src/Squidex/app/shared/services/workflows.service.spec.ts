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
    Resource,
    ResourceLinks,
    Version,
    WorkflowDto,
    WorkflowsDto,
    WorkflowsPayload,
    WorkflowStep,
    WorkflowTransition,
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

            let workflows: WorkflowsDto;

            workflowsService.getWorkflows('my-app').subscribe(result => {
                workflows = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(workflowsResponse(1, 2, 3),
                {
                    headers: {
                        etag: '2'
                    }
                });

            expect(workflows!).toEqual({ payload: createWorkflows(1, 2, 3), version: new Version('2') });
        }));

    it('should make a post request to assign a workflow',
        inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {

            const dto = createWorkflow(1);

            let workflows: WorkflowsDto;

            workflowsService.postWorkflows('my-app', dto, version).subscribe(result => {
                workflows = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(workflowsResponse(1, 2, 3), {
                headers: {
                    etag: '2'
                }
            });

            expect(workflows!).toEqual({ payload: createWorkflows(1, 2, 3), version: new Version('2') });
        }));

    it('should make a delete request to remove a workflow',
        inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {
                const resource: Resource = {
                    _links: {
                        'delete': { method: 'DELETE', href: '/api/apps/my-app/workflows/123' }
                    }
                };

                let workflows: WorkflowsDto;

                workflowsService.deleteWorkflow('my-app', resource, version).subscribe(result => {
                    workflows = result;
                });

                const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows/123');

                expect(req.request.method).toEqual('DELETE');
                expect(req.request.headers.get('If-Match')).toEqual(version.value);

                req.flush(workflowsResponse(1, 2, 3),
                    {
                        headers: {
                            etag: '2'
                        }
                    });

                expect(workflows!).toEqual({ payload: createWorkflows(1, 2, 3), version: new Version('2') });
        }));

});

describe('Workflow', () => {
    it('should create empty workflow', () => {
        const workflow = new WorkflowDto();

        expect(workflow.name).toEqual('Default');
    });

    it('should add step to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' }
            ],
            transitions: [],
            name: 'Default'
        });
    });

    it('should override settings if step already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('1', { color: 'red' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: 'red' }
            ],
            transitions: [],
            name: 'Default'
        });
    });

    it('should not remove step if locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .removeStep('1');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00', isLocked: true },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 === 2' }
            ],
            name: 'Default'
        });
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

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '2', color: '#ff0000' },
                { name: '3', color: '#0000ff' }
            ],
            transitions: [
                { from: '2', to: '3', expression: '2 === 3' }
            ],
            name: 'Default'
        });
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
                .renameStep('1', '4');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '4', color: '#00ff00' },
                { name: '2', color: '#ff0000' },
                { name: '3', color: '#0000ff' }
            ],
            transitions: [
                { from: '4', to: '2', expression: '1 === 2' },
                { from: '2', to: '4', expression: '2 === 1' },
                { from: '2', to: '3', expression: '2 === 3' }
            ],
            name: 'Default'
        });
    });

    it('should add transitions to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 === 2' },
                { from: '2', to: '1', expression: '2 === 1' }
            ],
            name: 'Default'
        });
    });

    it('should add remove transition from workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 1' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .removeTransition('1', '2');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '2', to: '1', expression: '2 === 1' }
            ],
            name: 'Default'
        });
    });

    it('should override settings if transition already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('1', '2', { expression: '1 !== 2' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 !== 2' }
            ],
            name: 'Default'
        });
    });

});

function simplify(value: any) {
    return JSON.parse(JSON.stringify(value));
}

function workflowsResponse(...ids: number[]) {

    return {
        items: ids.map(id => createWorkflow(id)),
        _links: {
            create: { method: 'POST', href: '/workflows' }
        },
        canCreate: true
    };


/*return {
        items: ids.map(id => ({
            id: `id${id}`,
            name: 'Published',
            color: 'red',
            isLocked: false,
            noUpdate: false,
            from: 'Draft',
            to: 'Published',
            expression: '1=1',
            role: 'Editor',

            _links: {
                update: { method: 'PUT', href: `/workflows/id${id}` }
            }
        })),
        _links: {
            create: { method: 'POST', href: '/workflows' }
        }
    };*/
}

export function createWorkflows(...ids: number[]): WorkflowsPayload {
    return {
        items: ids.map(id => createWorkflow(id)),
        _links: {
            create: { method: 'POST', href: '/workflows' }
        },

        canCreate: true
    };
}

export function createWorkflow(id: number) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/workflows/id${id}` }
    };

    const step: WorkflowStep = { name: 'Published', color: 'red', isLocked: false, noUpdate: false };

    const steps: WorkflowStep[] = [step];

    const transition: WorkflowTransition = { from: 'Draft', to: 'Published', expression: '1=1', role: 'Editor'};

    const transitions: WorkflowTransition[] = [transition];

    return new WorkflowDto(links, 'test', steps, transitions);
}