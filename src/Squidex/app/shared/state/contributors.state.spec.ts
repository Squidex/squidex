/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ContributorsService,
    ContributorsState,
    DialogService,
    versioned
} from '@app/shared/internal';

import { createContributors } from '../services/contributors.service.spec';

import { TestValues } from './_test-helpers';

describe('ContributorsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const oldContributors = createContributors(1, 2, 3);

    let dialogs: IMock<DialogService>;
    let contributorsService: IMock<ContributorsService>;
    let contributorsState: ContributorsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        contributorsService = Mock.ofType<ContributorsService>();
        contributorsState = new ContributorsState(contributorsService.object, appsState.object, dialogs.object);
    });

    afterEach(() => {
        contributorsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load contributors', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(versioned(version, oldContributors))).verifiable();

            contributorsState.load().subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual(oldContributors.contributors);
            expect(contributorsState.snapshot.isMaxReached).toBeFalsy();
            expect(contributorsState.snapshot.isLoaded).toBeTruthy();
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(versioned(version, oldContributors))).verifiable();

            contributorsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(versioned(version, oldContributors))).verifiable();

            contributorsState.load().subscribe();
        });

        it('should update contributors when user assigned', () => {
            const updated = createContributors(1, 2, 3);

            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            contributorsState.assign(request).subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual(oldContributors.contributors);
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        });

        it('should update contributors when contribution revoked', () => {
            const updated = createContributors(1, 2, 3);

            contributorsService.setup(x => x.deleteContributor(app, oldContributors.contributors[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            contributorsState.revoke(oldContributors.contributors[0]).subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual(oldContributors.contributors);
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        });
    });
});