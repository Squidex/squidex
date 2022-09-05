/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { BackupsService, BackupsState, DialogService } from '@app/shared/internal';
import { createBackup } from './../services/backups.service.spec';
import { TestValues } from './_test-helpers';

describe('BackupsState', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const backup1 = createBackup(12);
    const backup2 = createBackup(13);

    let dialogs: IMock<DialogService>;
    let backupsService: IMock<BackupsService>;
    let backupsState: BackupsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        backupsService = Mock.ofType<BackupsService>();
        backupsState = new BackupsState(appsState.object, backupsService.object, dialogs.object);
    });

    afterEach(() => {
        backupsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load backups', () => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => of({ items: [backup1, backup2] } as any)).verifiable();

            backupsState.load().subscribe();

            expect(backupsState.snapshot.backups).toEqual([backup1, backup2]);
            expect(backupsState.snapshot.isLoaded).toBeTruthy();
            expect(backupsState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => throwError(() => 'Service Error'));

            backupsState.load().pipe(onErrorResumeNext()).subscribe();

            expect(backupsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => of({ items: [backup1, backup2] } as any)).verifiable();

            backupsState.load(true, false).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should show notification on load error if silent is false', () => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => throwError(() => 'Service Error'));

            backupsState.load(true, false).pipe(onErrorResumeNext()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
        });

        it('should not show notification on load error if silent is true', () => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => throwError(() => 'Service Error'));

            backupsState.load(true, true).pipe(onErrorResumeNext()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            backupsService.setup(x => x.getBackups(app))
                .returns(() => of({ items: [backup1, backup2] } as any)).verifiable();

            backupsState.load().subscribe();
        });

        it('should not add backup to snapshot', () => {
            backupsService.setup(x => x.postBackup(app))
                .returns(() => of({})).verifiable();

            backupsState.start().subscribe();

            expect(backupsState.snapshot.backups.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should not remove backup from snapshot', () => {
            backupsService.setup(x => x.deleteBackup(app, backup1))
                .returns(() => of({})).verifiable();

            backupsState.delete(backup1).subscribe();

            expect(backupsState.snapshot.backups.length).toBe(2);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });
});
