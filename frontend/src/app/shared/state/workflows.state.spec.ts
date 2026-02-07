/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { AddWorkflowDto, DialogService, versioned, WorkflowDto, WorkflowsDto, WorkflowsService, WorkflowsState, WorkflowView } from '@app/shared/internal';
import { createWorkflows } from '../services/workflows.service.spec';
import { TestValues } from './_test-helpers';

describe('WorkflowsState', () => {
    const { app, appsState, newVersion, version } = TestValues;

    const oldWorkflows = createWorkflows('1', '2');

    let dialogs: IMock<DialogService>;
    let workflowsService: IMock<WorkflowsService>;
    let workflowsState: WorkflowsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        workflowsService = Mock.ofType<WorkflowsService>();
        workflowsState = new WorkflowsState(appsState.object, dialogs.object, workflowsService.object);
    });

    afterEach(() => {
        workflowsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load workflow', () => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => of(versioned(version, oldWorkflows))).verifiable();

            workflowsState.load().subscribe();

            expect(workflowsState.snapshot.isLoaded).toBeTruthy();
            expect(workflowsState.snapshot.isLoading).toBeFalsy();
            expect(workflowsState.snapshot.version).toEqual(version);
            expect(workflowsState.snapshot.workflows).toEqual(oldWorkflows.items);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => throwError(() => 'Service Error'));

            workflowsState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(workflowsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => of(versioned(version, oldWorkflows))).verifiable();

            workflowsState.load(true).subscribe();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => of(versioned(version, oldWorkflows))).verifiable();

            workflowsState.load().subscribe();
        });

        it('should update workflows if workflow added', () => {
            const updated = createWorkflows('1', '2', '3');

            const request = new AddWorkflowDto({ name: 'my-workflow' });

            workflowsService.setup(x => x.postWorkflow(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.add(request).subscribe();

            expectNewWorkflows(updated);
        });

        it('should update workflows if workflow updated', () => {
            const updated = createWorkflows('1', '2', '3');

            const request = { initial: '1' } as any;

            workflowsService.setup(x => x.putWorkflow(app, oldWorkflows.items[0], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.update(oldWorkflows.items[0], request).subscribe();

            expectNewWorkflows(updated);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should update workflows if workflow deleted', () => {
            const updated = createWorkflows('1', '2', '3');

            workflowsService.setup(x => x.deleteWorkflow(app, oldWorkflows.items[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.delete(oldWorkflows.items[0]).subscribe();

            expectNewWorkflows(updated);
        });

        function expectNewWorkflows(updated: WorkflowsDto) {
            expect(workflowsState.snapshot.workflows).toEqual(updated.items);
            expect(workflowsState.snapshot.version).toEqual(newVersion);
        }
    });
});

describe('WorkflowView', () => {
    const dto = new WorkflowDto({ id: 'id', steps: {}, initial: null!, _links: {} });

    it('should create empty workflow', () => {
        const workflow = new WorkflowView(dto);


        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: null,
            steps: {},
        });
    });

    it('should add step to workflow', () => {
        const workflow = new WorkflowView(dto)
            .setStep('Draft', { color: '#00ff00' });

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: 'Draft',
            steps: {
                'Draft': { transitions: {}, color: '#00ff00' },
            },
        });
    });

    it('should override settings if step already exists', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1', { color: '#00ff00', noUpdate: true })
            .setStep('1', { color: 'red' });

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '1',
            steps: {
                1: { transitions: {}, color: 'red', noUpdate: true },
            },
        });
    });

    it('should sort steps case invariant', () => {
        const workflow = new WorkflowView(dto)
            .setStep('Z')
            .setStep('a');

        expect(workflow.steps).toEqual([
            { name: 'a', values: {}, isLocked: false },
            { name: 'Z', values: {}, isLocked: false },
        ]);
    });

    it('should return same workflow if step to remove is locked', () => {
        const workflow = new WorkflowView(dto)
            .setStep('Published', { color: '#00ff00' });

        const updated = workflow.removeStep('Published');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to remove not found', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1');

        const updated = workflow.removeStep('3');

        expect(updated).toBe(workflow);
    });

    it('should remove step', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1', { color: '#00ff00' })
            .setStep('2', { color: '#ff0000' })
            .setStep('3', { color: '#0000ff' })
            .setTransition('1', '2', { expression: '1 === 2' })
            .setTransition('1', '3', { expression: '1 === 3' })
            .setTransition('2', '3', { expression: '2 === 3' })
            .removeStep('1');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '2',
            steps: {
                2: {
                    transitions: {
                        3: { expression: '2 === 3' },
                    },
                    color: '#ff0000',
                },
                3: { transitions: {}, color: '#0000ff' },
            },
        });
    });

    it('should make first non-locked step the initial step if initial removed', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('Published')
            .setStep('Public')
            .removeStep('1');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: 'Public',
            steps: {
                Published: { transitions: {} },
                Public: { transitions: {} },
            },
        });
    });

    it('should unset initial step if initial removed', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .removeStep('1');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: null,
            steps: {},
        });
    });

    it('should rename step', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1', { color: '#00ff00' })
            .setStep('2', { color: '#ff0000' })
            .setStep('3', { color: '#0000ff' })
            .setTransition('1', '2', { expression: '1 === 2' })
            .setTransition('2', '1', { expression: '2 === 1' })
            .setTransition('2', '3', { expression: '2 === 3' })
            .renameStep('1', 'a');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '1',
            steps: {
                a: {
                    transitions: {
                        2: { expression: '1 === 2' },
                    },
                    color: '#00ff00',
                },
                2: {
                    transitions: {
                        a: { expression: '2 === 1' },
                        3: { expression: '2 === 3' },
                    },
                    color: '#ff0000',
                },
                3: { transitions: {}, color: '#0000ff' },
            },
        });
    });

    it('should add transitions to workflow', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('1', '2', { expression: '1 === 2' })
            .setTransition('2', '1', { expression: '2 === 1' });

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '1',
            steps: {
                1: {
                    transitions: {
                        2: { expression: '1 === 2' },
                    },
                },
                2: {
                    transitions: {
                        1: { expression: '2 === 1' },
                    },
                },
            },
        });
    });

    it('should remove transition from workflow', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('1', '2', { expression: '1 === 2' })
            .setTransition('2', '1', { expression: '2 === 1' })
            .removeTransition('1', '2');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '1',
            steps: {
                1: { transitions: {} },
                2: {
                    transitions: {
                        1: { expression: '2 === 1' },
                    },
                },
            },
        });
    });

    it('should override settings if transition already exists', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('2', '1', { expression: '2 === 1', roles: ['Role'] })
            .setTransition('2', '1', { expression: '2 !== 1' });

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '1',
            steps: {
                1: { transitions: {} },
                2: {
                    transitions: {
                        1: { expression: '2 !== 1', roles: ['Role'] },
                    },
                },
            },
        });
    });

    it('should return same workflow if transition to update not found by from step', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('1', '2');

        const updated = workflow.setTransition('3', '2', { roles: ['Role'] });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition has invalid steps', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('1', '2');

        const updated = workflow.setTransition('1', '3', { roles: ['Role'] });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition to remove not', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setTransition('1', '2');

        const updated = workflow.removeTransition('1', '3');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to make initial is locked', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('Published', { color: '#00ff00' });

        const updated = workflow.setInitial('Published');

        expect(updated).toBe(workflow);
    });

    it('should set initial step', () => {
        const workflow = new WorkflowView(dto)
            .setStep('1')
            .setStep('2')
            .setInitial('2');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: '2',
            steps: {
                1: { transitions: {} },
                2: { transitions: {} },
            },
        });
    });

    it('should rename workflow', () => {
        const workflow = new WorkflowView(dto)
            .changeName('name');

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: null,
            steps: {},
            name: 'name',
        });
    });

    it('should update schemaIds', () => {
        const workflow = new WorkflowView(dto)
            .changeSchemaIds(['1', '2']);

        expect(workflow.toUpdate().toJSON()).toEqual({
            initial: null,
            steps: {},
            schemaIds: ['1', '2'],
        });
    });
});
