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
    ContributorsPayload,
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

    let allIds: number[] = [];

    for (let i = 1; i <= 20; i++) {
        allIds.push(i);
    }

    const oldContributors = createContributors(...allIds);

    let dialogs: IMock<DialogService>;
    let contributorsService: IMock<ContributorsService>;
    let contributorsState: ContributorsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        contributorsService = Mock.ofType<ContributorsService>();
        contributorsState = new ContributorsState(contributorsService.object, appsState.object, dialogs.object);

        contributorsService.setup(x => x.getContributors(app))
            .returns(() => of(versioned(version, oldContributors))).verifiable();
    });

    afterEach(() => {
        contributorsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load contributors', () => {
            contributorsState.load().subscribe();

            expect(contributorsState.snapshot.contributors).toEqual(oldContributors.items);
            expect(contributorsState.snapshot.isLoaded).toBeTruthy();
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should only current page of contributors', () => {
            contributorsState.load().subscribe();

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(0, 10));
            expect(contributorsState.snapshot.page).toEqual(0);
        });

        it('should show next of contributors when going next', () => {
            contributorsState.load().subscribe();
            contributorsState.goNext();

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(10, 20));
            expect(contributorsState.snapshot.page).toEqual(1);
        });

        it('should show next of contributors when going prev', () => {
            contributorsState.load().subscribe();
            contributorsState.goNext();
            contributorsState.goPrev();

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(0, 10));
            expect(contributorsState.snapshot.page).toEqual(0);
        });

        it('should show filtered contributors when searching', () => {
            contributorsState.load().subscribe();
            contributorsState.search('4');

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(createContributors(4, 14).items);
            expect(contributorsState.snapshot.page).toEqual(0);
        });

        it('should show notification on load when reload is true', () => {
            contributorsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            contributorsState.load().subscribe();
        });

        it('should update contributors when user assigned', () => {
            const updated = createContributors(5, 6);

            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            contributorsState.assign(request).subscribe();

            expectNewContributors(updated);
        });

        it('should update contributors when contribution revoked', () => {
            const updated = createContributors(5, 6);

            contributorsService.setup(x => x.deleteContributor(app, oldContributors.items[0], version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            contributorsState.revoke(oldContributors.items[0]).subscribe();

            expectNewContributors(updated);
        });

        function expectNewContributors(updated: ContributorsPayload) {
            expect(contributorsState.snapshot.contributors).toEqual(updated.items);
            expect(contributorsState.snapshot.maxContributors).toBe(updated.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(newVersion);
        }
    });
});