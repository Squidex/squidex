/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { empty, of, throwError } from 'rxjs';
import { catchError, onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ContributorDto,
    ContributorsPayload,
    ContributorsService,
    ContributorsState,
    DialogService,
    LocalStoreService,
    Pager,
    versioned
} from '@app/shared/internal';

import { createContributors } from '../services/contributors.service.spec';

import { ErrorDto } from '@app/framework';
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
    let localStore: IMock<LocalStoreService>;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        localStore = Mock.ofType<LocalStoreService>();

        contributorsService = Mock.ofType<ContributorsService>();
        contributorsService.setup(x => x.getContributors(app))
            .returns(() => of(versioned(version, oldContributors))).verifiable();

        contributorsState = new ContributorsState(appsState.object, contributorsService.object, dialogs.object, localStore.object);
    });

    afterEach(() => {
        contributorsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load contributors', () => {
            contributorsState.load().subscribe();

            expect(contributorsState.snapshot.contributors).toEqual(oldContributors.items);
            expect(contributorsState.snapshot.contributorsPager).toEqual(new Pager(20, 0, 10));
            expect(contributorsState.snapshot.isLoaded).toBeTruthy();
            expect(contributorsState.snapshot.isLoading).toBeFalsy();
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading when loading failed', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => throwError('error'));

            contributorsState.load().pipe(onErrorResumeNext()).subscribe();

            expect(contributorsState.snapshot.isLoading).toBeFalsy();
        });

        it('should only show current page of contributors', () => {
            contributorsState.load().subscribe();

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(0, 10));
            expect(contributorsState.snapshot.contributorsPager).toEqual(new Pager(20, 0, 10));
        });

        it('should show with new pagination when paging', () => {
            contributorsState.load().subscribe();
            contributorsState.setPager(new Pager(20, 1, 10));

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(10, 20));
            expect(contributorsState.snapshot.contributorsPager).toEqual(new Pager(20, 1, 10));
        });

        it('should update page size in local store', () => {
            contributorsState.load().subscribe();
            contributorsState.setPager(new Pager(0, 0, 50));

            localStore.verify(x => x.setInt('contributors.pageSize', 50), Times.atLeastOnce());

            expect().nothing();
        });

        it('should show filtered contributors when searching', () => {
            contributorsState.load().subscribe();
            contributorsState.search('4');

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsPaged.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(createContributors(4, 14).items);
            expect(contributorsState.snapshot.contributorsPager.page).toEqual(0);
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

        it('should return proper error when user to add does not exist', () => {
            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => throwError(new ErrorDto(404, '404')));

            let error: ErrorDto;

            contributorsState.assign(request).pipe(
                catchError(err => {
                    error = err;

                    return empty();
                })
            ).subscribe();

            expect(error!.message).toBe('The user does not exist.');
        });

        it('should return original error when not a 404', () => {
            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => throwError(new ErrorDto(500, '500')));

            let error: ErrorDto;

            contributorsState.assign(request).pipe(
                catchError(err => {
                    error = err;

                    return empty();
                })
            ).subscribe();

            expect(error!.message).toBe('500');
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