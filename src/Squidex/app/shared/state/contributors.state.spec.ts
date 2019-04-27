/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ContributorDto,
    ContributorsDto,
    ContributorsService,
    ContributorsState,
    DialogService,
    Versioned
} from './../';

import { TestValues } from './_test-helpers';

describe('ContributorsState', () => {
    const {
        app,
        appsState,
        authService,
        newVersion,
        userId,
        version
    } = TestValues;

    const oldContributors = [
        new ContributorDto('id1', 'Developer'),
        new ContributorDto(userId, 'Developer')
    ];

    let dialogs: IMock<DialogService>;
    let contributorsService: IMock<ContributorsService>;
    let contributorsState: ContributorsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        contributorsService = Mock.ofType<ContributorsService>();
        contributorsState = new ContributorsState(contributorsService.object, appsState.object, authService.object, dialogs.object);
    });

    afterEach(() => {
        contributorsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load contributors', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(new ContributorsDto(oldContributors, 3, version))).verifiable();

            contributorsState.load().subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual([
                { isCurrentUser: false, contributor: oldContributors[0] },
                { isCurrentUser: true,  contributor: oldContributors[1] }
            ]);
            expect(contributorsState.snapshot.isMaxReached).toBeFalsy();
            expect(contributorsState.snapshot.isLoaded).toBeTruthy();
            expect(contributorsState.snapshot.maxContributors).toBe(3);
            expect(contributorsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(new ContributorsDto(oldContributors, 3, version))).verifiable();

            contributorsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => of(new ContributorsDto(oldContributors, 3, version))).verifiable();

            contributorsState.load().subscribe();
        });

        it('should add contributor to snapshot when assigned', () => {
            const newContributor = new ContributorDto('id3', 'Developer');

            const request = { contributorId: 'mail2stehle@gmail.com', role: newContributor.role };
            const response = { contributorId: newContributor.contributorId, isCreated: true };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => of(new Versioned(newVersion, response))).verifiable();

            contributorsState.assign(request).subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual([
                { isCurrentUser: false, contributor: oldContributors[0] },
                { isCurrentUser: true,  contributor: oldContributors[1] },
                { isCurrentUser: false,  contributor: newContributor }
            ]);
            expect(contributorsState.snapshot.isMaxReached).toBeTruthy();
            expect(contributorsState.snapshot.maxContributors).toBe(3);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        });

        it('should update contributor in snapshot when assigned and already added', () => {
            const newContributor = new ContributorDto(userId, 'Owner');

            const request = { ...newContributor };
            const response = { contributorId: newContributor.contributorId, isCreated: true };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => of(new Versioned(newVersion, response))).verifiable();

            contributorsState.assign(request).subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual([
                { isCurrentUser: false, contributor: oldContributors[0] },
                { isCurrentUser: true,  contributor: newContributor }
            ]);
            expect(contributorsState.snapshot.isMaxReached).toBeFalsy();
            expect(contributorsState.snapshot.maxContributors).toBe(3);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        });

        it('should remove contributor from snapshot when revoked', () => {
            contributorsService.setup(x => x.deleteContributor(app, oldContributors[0].contributorId, version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            contributorsState.revoke(oldContributors[0]).subscribe();

            expect(contributorsState.snapshot.contributors.values).toEqual([
                { isCurrentUser: true, contributor: oldContributors[1] }
            ]);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        });
    });
});