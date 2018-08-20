/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    BackupDto,
    BackupsService,
    BackupsState,
    DateTime,
    DialogService
} from '@app/shared';

describe('BackupsState', () => {
    const app = 'my-app';

    const oldBackups = [
        new BackupDto('id1', DateTime.now(), null, 1, 1, 'Started'),
        new BackupDto('id2', DateTime.now(), null, 2, 2, 'Started')
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let backupsService: IMock<BackupsService>;
    let backupsState: BackupsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        backupsService = Mock.ofType<BackupsService>();

        backupsService.setup(x => x.getBackups(app))
            .returns(() => of(oldBackups));

        backupsState = new BackupsState(appsState.object, backupsService.object, dialogs.object);
        backupsState.load().subscribe();
    });

    it('should load backups', () => {
        expect(backupsState.snapshot.backups.values).toEqual(oldBackups);
        expect(backupsState.snapshot.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        backupsState.load(true, false).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should show notification on load error when silent is true', () => {
        backupsService.setup(x => x.getBackups(app))
            .returns(() => throwError({}));

        backupsState.load(true, true).pipe(onErrorResumeNext()).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
    });

    it('should not show notification on load error when flag is false', () => {
        backupsService.setup(x => x.getBackups(app))
            .returns(() => throwError({}));

        backupsState.load().pipe(onErrorResumeNext()).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
    });

    it('should not add backup to snapshot', () => {
        backupsService.setup(x => x.postBackup(app))
            .returns(() => of({}));

        backupsState.start().subscribe();

        expect(backupsState.snapshot.backups.length).toBe(2);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should not remove backup from snapshot', () => {
        backupsService.setup(x => x.deleteBackup(app, oldBackups[0].id))
            .returns(() => of({}));

        backupsState.delete(oldBackups[0]).subscribe();

        expect(backupsState.snapshot.backups.length).toBe(2);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });
});