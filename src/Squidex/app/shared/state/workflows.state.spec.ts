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
    WorkflowPayload,
    WorkflowsService,
    WorkflowsState
} from '@app/shared/internal';

import { createWorkflow } from '../services/workflows.service.spec';

import { TestValues } from './_test-helpers';

describe('WorkflowsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldWorkflow = createWorkflow('test');

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
            workflowsService.setup(x => x.getWorkflow(app))
                .returns(() => of(versioned(version, oldWorkflow))).verifiable();

            workflowsState.load().subscribe();

            expect(workflowsState.snapshot.workflow).toEqual(oldWorkflow.workflow);
            expect(workflowsState.snapshot.isLoaded).toBeTruthy();
            expect(workflowsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            workflowsService.setup(x => x.getWorkflow(app))
                .returns(() => of(versioned(version, oldWorkflow))).verifiable();

            workflowsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            workflowsService.setup(x => x.getWorkflow(app))
                .returns(() => of(versioned(version, oldWorkflow))).verifiable();

            workflowsState.load().subscribe();
        });

        it('should update workflows when saved', () => {
            const updated = createWorkflow('updated');

            const request = oldWorkflow.workflow.serialize();

            workflowsService.setup(x => x.putWorkflow(app, oldWorkflow, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            workflowsState.save(oldWorkflow.workflow).subscribe();

            expectNewWorkflows(updated);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        function expectNewWorkflows(updated: WorkflowPayload) {
            expect(workflowsState.snapshot.workflow).toEqual(updated.workflow);
            expect(workflowsState.snapshot.version).toEqual(newVersion);
        }
    });
});