/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, Versioned, VersionTag, WorkflowDto, WorkflowsDto, WorkflowsService } from '@app/shared/internal';
import { AddWorkflowDto, ResourceLinkDto, UpdateWorkflowDto, WorkflowStepDto, WorkflowTransitionDto } from '../model';

describe('WorkflowsService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
                WorkflowsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make a get request to get app workflows', inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {
        let workflows: Versioned<WorkflowsDto>;
        workflowsService.getWorkflows('my-app').subscribe(result => {
            workflows = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush(workflowsResponse('1', '2'), {
            headers: {
                etag: '2',
            },
        });

        expect(workflows!).toEqual({ payload: createWorkflows('1', '2'), version: new VersionTag('2') });
    }));

    it('should make a post request to create a workflow', inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {
        const dto = new AddWorkflowDto({ name: 'New' });

        let workflows: Versioned<WorkflowsDto>;
        workflowsService.postWorkflow('my-app', dto, version).subscribe(result => {
            workflows = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(workflowsResponse('1', '2'), {
            headers: {
                etag: '2',
            },
        });

        expect(workflows!).toEqual({ payload: createWorkflows('1', '2'), version: new VersionTag('2') });
    }));

    it('should make a put request to update a workflow', inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {
        const dto = new UpdateWorkflowDto({ initial: 'A', steps: {} });

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app/workflows/123' },
            },
        };

        let workflows: Versioned<WorkflowsDto>;
        workflowsService.putWorkflow('my-app', resource, dto, version).subscribe(result => {
            workflows = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(workflowsResponse('1', '2'), {
            headers: {
                etag: '2',
            },
        });

        expect(workflows!).toEqual({ payload: createWorkflows('1', '2'), version: new VersionTag('2') });
    }));

    it('should make a delete request to delete a workflow', inject([WorkflowsService, HttpTestingController], (workflowsService: WorkflowsService, httpMock: HttpTestingController) => {
        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/workflows/123' },
            },
        };

        let workflows: Versioned<WorkflowsDto>;
        workflowsService.deleteWorkflow('my-app', resource, version).subscribe(result => {
            workflows = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/workflows/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush(workflowsResponse('1', '2'), {
            headers: {
                etag: '2',
            },
        });

        expect(workflows!).toEqual({ payload: createWorkflows('1', '2'), version: new VersionTag('2') });
    }));

    function workflowsResponse(...names: string[]) {
        return {
            errors: [
                'Error1',
                'Error2',
            ],
            items: names.map(workflowResponse),
            _links: {
                create: { method: 'POST', href: '/workflows' },
            },
        };
    }

    function workflowResponse(name: string) {
        return {
            id: `id_${name}`,
            name: `name_${name}`,
            initial: `${name}1`,
            schemaIds: [`schema_${name}`],
            steps: {
                [`${name}1`]: {
                    transitions: {
                        [`${name}2`]: {
                            expression: 'Expression1', roles: ['Role1'],
                        },
                    },
                    color: `${name}1`, noUpdate: true,
                },
                [`${name}2`]: {
                    transitions: {
                        [`${name}1`]: {
                            expression: 'Expression2', roles: ['Role2'],
                        },
                    },
                    color: `${name}2`, noUpdate: true,
                },
            },
            _links: {
                update: { method: 'PUT', href: `/workflows/${name}` },
            },
        };
    }
});

export function createWorkflows(...names: ReadonlyArray<string>) {
    return new WorkflowsDto({
        errors: [
            'Error1',
            'Error2',
        ],
        items: names.map(createWorkflow),
        _links: {
            create: new ResourceLinkDto({ method: 'POST', href: '/workflows' }),
        },
    });
}

export function createWorkflow(name: string) {
    return new WorkflowDto({
        id: `id_${name}`,
        name: `name_${name}`,
        initial: `${name}1`,
        schemaIds: [`schema_${name}`],
        steps: {
            [`${name}1`]: new WorkflowStepDto({
                transitions: {
                    [`${name}2`]: new WorkflowTransitionDto({
                        expression: 'Expression1', roles: ['Role1'],
                    }),
                },
                color: `${name}1`, noUpdate: true,
            }),
            [`${name}2`]: new WorkflowStepDto({
                transitions: {
                    [`${name}1`]: new WorkflowTransitionDto({
                        expression: 'Expression2', roles: ['Role2'],
                    }),
                },
                color: `${name}2`, noUpdate: true,
            }),
        },
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/workflows/${name}` }),
        },
    });
}
