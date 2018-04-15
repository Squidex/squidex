/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    BackupDto,
    BackupsState,
    BackupsService,
    DateTime,
    DialogService
 } from '@app/shared';

describe('BackupsState', () => {
    const app = 'my-app';

    const oldBackups = [
        new BackupDto('id1', DateTime.now(), null, 1, 1, false),
        new BackupDto('id2', DateTime.now(), null, 2, 2, false)
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
            .returns(() => Observable.of(oldBackups));

        backupsState = new BackupsState(appsState.object, backupsService.object, dialogs.object);
        backupsState.load().subscribe();
    });

    it('should load clients', () => {
        expect(backupsState.snapshot.backups.values).toEqual(oldBackups);
    });

    it('should not add backup to snapshot', () => {
        backupsService.setup(x => x.postBackup(app))
            .returns(() => Observable.of({}));

        backupsState.start().subscribe();

        expect(backupsState.snapshot.backups.length).toBe(2);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should not remove backup from snapshot', () => {
        backupsService.setup(x => x.deleteBackup(app, oldBackups[0].id))
            .returns(() => Observable.of({}));

        backupsState.delete(oldBackups[0]).subscribe();

        expect(backupsState.snapshot.backups.length).toBe(2);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });
});