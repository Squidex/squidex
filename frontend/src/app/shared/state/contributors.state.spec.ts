/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EMPTY, of, throwError } from 'rxjs';
import { catchError, onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { ErrorDto } from '@app/framework';
import { ContributorDto, ContributorsPayload, ContributorsService, ContributorsState, DialogService, versioned } from '@app/shared/internal';
import { createContributors } from './../services/contributors.service.spec';
import { TestValues } from './_test-helpers';

describe('ContributorsState', () => {
    const {
        app,
        appsState,
        newVersion,
        version,
    } = TestValues;

    const allIds: number[] = [];

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
        contributorsService.setup(x => x.getContributors(app))
            .returns(() => of(versioned(version, oldContributors))).verifiable(Times.atLeastOnce());

        contributorsState = new ContributorsState(appsState.object, contributorsService.object, dialogs.object);
    });

    afterEach(() => {
        contributorsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load contributors', () => {
            contributorsState.load().subscribe();

            expect(contributorsState.snapshot.contributors).toEqual(oldContributors.items);
            expect(contributorsState.snapshot.isLoaded).toBeTruthy();
            expect(contributorsState.snapshot.isLoading).toBeFalsy();
            expect(contributorsState.snapshot.total).toEqual(20);
            expect(contributorsState.snapshot.maxContributors).toBe(oldContributors.maxContributors);
            expect(contributorsState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            contributorsService.setup(x => x.getContributors(app))
                .returns(() => throwError(() => 'Service Error'));

            contributorsState.load().pipe(onErrorResumeNext()).subscribe();

            expect(contributorsState.snapshot.isLoading).toBeFalsy();
        });

        it('should not load if already loaded', () => {
            contributorsState.load(true).subscribe();
            contributorsState.loadIfNotLoaded().subscribe();

            expect().nothing();
        });

        it('should only show current page of contributors', () => {
            contributorsState.load().subscribe();

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsFiltered.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(0, 10));
            expect(contributorsState.snapshot.page).toEqual(0);
            expect(contributorsState.snapshot.pageSize).toEqual(10);
        });

        it('should show with new pagination if paging', () => {
            contributorsState.load().subscribe();
            contributorsState.page({ page: 1, pageSize: 10 });

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsFiltered.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(oldContributors.items.slice(10, 20));
            expect(contributorsState.snapshot.page).toEqual(1);
            expect(contributorsState.snapshot.pageSize).toEqual(10);
        });

        it('should show filtered contributors if searching', () => {
            contributorsState.load().subscribe();
            contributorsState.search('4');

            let contributors: ReadonlyArray<ContributorDto>;

            contributorsState.contributorsFiltered.subscribe(result => {
                contributors = result;
            });

            expect(contributors!).toEqual(createContributors(4, 14).items);
            expect(contributorsState.snapshot.page).toEqual(0);
            expect(contributorsState.snapshot.pageSize).toEqual(10);
        });

        it('should show notification on load if reload is true', () => {
            contributorsState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            contributorsState.load().subscribe();
        });

        it('should update contributors if user assigned', () => {
            const updated = createContributors(5, 6);

            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => of(versioned(newVersion, updated))).verifiable();

            contributorsState.assign(request).subscribe();

            expectNewContributors(updated);
        });

        it('should return proper error if user to add does not exist', () => {
            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => throwError(() => new ErrorDto(404, '404')));

            let error: ErrorDto;

            contributorsState.assign(request).pipe(
                catchError(err => {
                    error = err;

                    return EMPTY;
                }),
            ).subscribe();

            expect(error!.message).toBe('i18n:contributors.userNotFound');
        });

        it('should return original error if not a 404', () => {
            const request = { contributorId: 'mail2stehle@gmail.com', role: 'Developer' };

            contributorsService.setup(x => x.postContributor(app, request, version))
                .returns(() => throwError(() => new ErrorDto(500, '500')));

            let error: ErrorDto;

            contributorsState.assign(request).pipe(
                catchError(err => {
                    error = err;

                    return EMPTY;
                }),
            ).subscribe();

            expect(error!.message).toBe('500');
        });

        it('should update contributors if contribution revoked', () => {
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
