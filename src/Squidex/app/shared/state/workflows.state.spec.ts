/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    DialogService,
    versioned,
    WorkflowsPayload,
    WorkflowsService,
    WorkflowsState
} from '@app/shared/internal';

import { createWorkflows } from '../services/workflows.service.spec';

import { TestValues } from './_test-helpers';

describe('WorkflowsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldWorkflows = createWorkflows('1', '2');

    let dialogs: IMock<DialogService>;
    let workflowsService: IMock<WorkflowsService>;
    let workflowsState: WorkflowsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        workflowsService = Mock.ofType<WorkflowsService>();
        workflowsState = new WorkflowsState(workflowsService.object, appsState.object, dialogs.object);
    });

    afterEach(() => {
        workflowsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load workflow', () => {
            workflowsService.setup(x => x.getWorkflows(app))
                .returns(() => of(versioned(version, oldWorkflows))).verifiable();

            workflowsState.load().subscribe();

            expect(workflowsState.snapshot.workflows.values).toEqual(oldWorkflows.items);
            expect(workflowsState.snapshot.isLoaded).toBeTruthy();
            expect(workflowsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
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

        it('should update workflows when workflow added', () => {
            const updated = createWorkflows('1', '2', '3');

            workflowsService.setup(x => x.postWorkflow(app, { name: 'my-workflow' }, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.add('my-workflow' ).subscribe();

            expectNewWorkflows(updated);
        });

        it('should update workflows when workflow updated', () => {
            const updated = createWorkflows('1', '2', '3');

            const request = oldWorkflows.items[0].serialize();

            workflowsService.setup(x => x.putWorkflow(app, oldWorkflows.items[0], request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.update(oldWorkflows.items[0]).subscribe();

            expectNewWorkflows(updated);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should update workflows when workflow deleted', () => {
            const updated = createWorkflows('1', '2', '3');

            workflowsService.setup(x => x.deleteWorkflow(app, oldWorkflows.items[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.delete(oldWorkflows.items[0]).subscribe();

            expectNewWorkflows(updated);
        });

        function expectNewWorkflows(updated: WorkflowsPayload) {
            expect(workflowsState.snapshot.workflows.values).toEqual(updated.items);
            expect(workflowsState.snapshot.version).toEqual(newVersion);
        }
    });
});