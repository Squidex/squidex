/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, JobsService, JobsState } from '@app/shared/internal';
import { createJob } from '../services/jobs.service.spec';
import { TestValues } from './_test-helpers';

describe('JobsState', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const job1 = createJob(12);
    const job2 = createJob(13);

    let dialogs: IMock<DialogService>;
    let jobsService: IMock<JobsService>;
    let jobsState: JobsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        jobsService = Mock.ofType<JobsService>();
        jobsState = new JobsState(appsState.object, jobsService.object, dialogs.object);
    });

    afterEach(() => {
        jobsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load jobs', () => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => of({ items: [job1, job2] } as any)).verifiable();

            jobsState.load().subscribe();

            expect(jobsState.snapshot.jobs).toEqual([job1, job2]);
            expect(jobsState.snapshot.isLoaded).toBeTruthy();
            expect(jobsState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => throwError(() => 'Service Error'));

            jobsState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(jobsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => of({ items: [job1, job2] } as any)).verifiable();

            jobsState.load(true, false).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should show notification on load error if silent is false', () => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => throwError(() => 'Service Error'));

            jobsState.load(true, false).pipe(onErrorResumeNextWith()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
        });

        it('should not show notification on load error if silent is true', () => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => throwError(() => 'Service Error'));

            jobsState.load(true, true).pipe(onErrorResumeNextWith()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            jobsService.setup(x => x.getJobs(app))
                .returns(() => of({ items: [job1, job2] } as any)).verifiable();

            jobsState.load().subscribe();
        });

        it('should not add job to snapshot', () => {
            jobsService.setup(x => x.postBackup(app))
                .returns(() => of({})).verifiable();

            jobsState.startBackup().subscribe();

            expect(jobsState.snapshot.jobs.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should not remove job from snapshot', () => {
            jobsService.setup(x => x.deleteJob(app, job1))
                .returns(() => of({})).verifiable();

            jobsState.delete(job1).subscribe();

            expect(jobsState.snapshot.jobs.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });
});
