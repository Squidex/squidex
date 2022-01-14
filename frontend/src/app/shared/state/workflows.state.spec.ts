/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, versioned, WorkflowsPayload, WorkflowsService, WorkflowsState } from '@app/shared/internal';
import { createWorkflows } from './../services/workflows.service.spec';
import { TestValues } from './_test-helpers';

describe('WorkflowsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

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

            workflowsState.load().pipe(onErrorResumeNext()).subscribe();

            expect(workflowsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => of(versioned(version, oldWorkflows))).verifiable();

            workflowsState.load(true).subscribe();

            expect().nothing();

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

            workflowsService.setup(x => x.postWorkflow(app, { name: 'my-workflow' }, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.add('my-workflow').subscribe();

            expectNewWorkflows(updated);
        });

        it('should update workflows if workflow updated', () => {
            const updated = createWorkflows('1', '2', '3');

            const request = oldWorkflows.items[0].serialize();

            workflowsService.setup(x => x.putWorkflow(app, oldWorkflows.items[0], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.update(oldWorkflows.items[0]).subscribe();

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

        function expectNewWorkflows(updated: WorkflowsPayload) {
            expect(workflowsState.snapshot.workflows).toEqual(updated.items);
            expect(workflowsState.snapshot.version).toEqual(newVersion);
        }
    });
});
