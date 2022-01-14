/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { AppsService, AppsState, DialogService } from '@app/shared/internal';
import { createApp, createAppSettings } from './../services/apps.service.spec';

describe('AppsState', () => {
    const app1 = createApp(1);
    const app2 = createApp(2);
    const app1Settings = createAppSettings(1);

    let dialogs: IMock<DialogService>;
    let appsService: IMock<AppsService>;
    let appsState: AppsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsService = Mock.ofType<AppsService>();

        appsService.setup(x => x.getApps())
            .returns(() => of([app1, app2])).verifiable(Times.atLeastOnce());

        appsService.setup(x => x.getSettings(It.isAnyString()))
            .returns(() => of(app1Settings));

        appsState = new AppsState(appsService.object, dialogs.object);
        appsState.load().subscribe();
    });

    afterEach(() => {
        appsService.verifyAll();
    });

    it('should load apps', () => {
        expect(appsState.snapshot.apps).toEqual([app1, app2]);
    });

    it('should select app', async () => {
        const appSelect = await firstValueFrom(appsState.select(app1.name));

        expect(appSelect).toBe(app1);
        expect(appsState.snapshot.selectedApp).toBe(app1);
        expect(appsState.snapshot.selectedSettings).not.toBeNull();

        appsService.verify(x => x.getSettings(app1.name), Times.once());
    });

    it('should reload settings if app selected', () => {
        appsState.select(app1.name).subscribe();
        appsState.loadSettings();

        appsService.verify(x => x.getSettings(app1.name), Times.exactly(2));

        expect().nothing();
    });

    it('should not load settings if no app selected', () => {
        appsState.loadSettings();

        appsService.verify(x => x.getSettings(app1.name), Times.never());

        expect().nothing();
    });

    it('should return null on select if unselecting app', async () => {
        const appSelected = await firstValueFrom(appsState.select(null));

        expect(appSelected).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
        expect(appsState.snapshot.selectedSettings).toBeNull();

        appsService.verify(x => x.getSettings(It.isAnyString()), Times.never());
    });

    it('should return null on select if app is not found', async () => {
        appsService.setup(x => x.getApp('unknown'))
            .returns(() => throwError(() => 'Service Error'));

        const appSelected = await firstValueFrom(appsState.select('unknown'));

        expect(appSelected).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return new app if loaded', async () => {
        const newApp = createApp(1, '_new');

        appsService.setup(x => x.getApp(app1.name))
            .returns(() => of(newApp));

        const appSelected = await firstValueFrom(appsState.loadApp(app1.name));

        expect(appSelected).toEqual(newApp);
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should add app to snapshot if created', () => {
        const updated = createApp(3, '_new');

        const request = { ...updated };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(updated)).verifiable();

        appsState.create(request).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, app2, updated]);
    });

    it('should update app if updated', () => {
        const request = {};

        const updated = createApp(2, '_new');

        appsService.setup(x => x.putApp(app2.name, app2, request, app2.version))
            .returns(() => of(updated)).verifiable();

        appsState.update(app2, request).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should update app if image uploaded', () => {
        const updated = createApp(2, '_new');

        const file = <File>{};

        appsService.setup(x => x.postAppImage(app2.name, app2, file, app2.version))
            .returns(() => of(50, 60, updated)).verifiable();

        appsState.uploadImage(app2, file).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should update app if image removed', () => {
        const updated = createApp(2, '_new');

        appsService.setup(x => x.deleteAppImage(app2.name, app2, app2.version))
            .returns(() => of(updated)).verifiable();

        appsState.removeImage(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should remove app from snapshot if left', () => {
        appsService.setup(x => x.leaveApp(app2.name, app2))
            .returns(() => of({})).verifiable();

        appsState.leave(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1]);
    });

    it('should remove app from snapshot if archived', () => {
        appsService.setup(x => x.deleteApp(app2.name, app2))
            .returns(() => of({})).verifiable();

        appsState.delete(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1]);
    });

    describe('Selection', () => {
        beforeEach(() => {
            appsState.select(app1.name).subscribe();
        });

        it('should update selected app if reloaded', () => {
            const newApps = [
                createApp(1, '_new'),
                createApp(2, '_new'),
            ];

            appsService.setup(x => x.getApps())
                .returns(() => of(newApps));

            appsState.load().subscribe();

            expect(appsState.snapshot.selectedApp).toEqual(newApps[0]);
        });

        it('should update selected app settings if updated', () => {
            const updated = createAppSettings(1, '_new');

            appsService.setup(x => x.putSettings(app1.name, app1Settings, {} as any, app1Settings.version))
                .returns(() => of(updated)).verifiable();

            appsState.updateSettings(app1Settings, {} as any).subscribe();

            expect(appsState.snapshot.selectedSettings).toBe(updated);
        });

        it('should update selected app if updated', () => {
            const request = {};

            const updated = createApp(1, '_new');

            appsService.setup(x => x.putApp(app1.name, app1, request, app1.version))
                .returns(() => of(updated)).verifiable();

            appsState.update(app1, request).subscribe();

            expect(appsState.snapshot.selectedApp).toEqual(updated);
        });

        it('should remove selected app from snapshot if archived', () => {
            appsService.setup(x => x.deleteApp(app1.name, app1))
                .returns(() => of(true)).verifiable();

            appsState.delete(app1).subscribe();

            expect(appsState.snapshot.selectedApp).toBeNull();
        });
    });
});
