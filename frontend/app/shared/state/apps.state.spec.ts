/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AppDto, AppsService, AppsState, DialogService } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { createApp } from './../services/apps.service.spec';

describe('AppsState', () => {
    const app1 = createApp(1);
    const app2 = createApp(2);

    let dialogs: IMock<DialogService>;
    let appsService: IMock<AppsService>;
    let appsState: AppsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsService = Mock.ofType<AppsService>();

        appsService.setup(x => x.getApps())
            .returns(() => of([app1, app2])).verifiable();

        appsState = new AppsState(appsService.object, dialogs.object);
        appsState.load().subscribe();
    });

    afterEach(() => {
        appsService.verifyAll();
    });

    it('should load apps', () => {
        expect(appsState.snapshot.apps).toEqual([app1, app2]);
    });

    it('should select app', () => {
        let selectedApp: AppDto;

        appsState.select(app1.name).subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBe(app1);
        expect(appsState.snapshot.selectedApp).toBe(app1);
    });

    it('should return null on select when unselecting app', () => {
        let selectedApp: AppDto;

        appsState.select(null).subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return new app when loaded', () => {
        const newApp = createApp(1, '_new');

        appsService.setup(x => x.getApp(app1.name))
            .returns(() => of(newApp));

        let selectedApp: AppDto;

        appsState.loadApp(app1.name).subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toEqual(newApp);
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return new app when reloaded', () => {
        const newApp = createApp(1, '_new');

        appsService.setup(x => x.getApp(app1.name))
            .returns(() => of(newApp));

        appsState.select(app1.name).subscribe();
        appsState.reloadSelected();

        expect(appsState.snapshot.selectedApp).toEqual(newApp);
    });

    it('should return null on select when app is not found', () => {
        let selectedApp: AppDto;

        appsService.setup(x => x.getApp('unknown'))
            .returns(() => throwError(new Error('404')));

        appsState.select('unknown').subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should add app to snapshot when created', () => {
        const updated = createApp(3, '_new');

        const request = { ...updated };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(updated)).verifiable();

        appsState.create(request).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, app2, updated]);
    });

    it('should update app in snapshot when updated', () => {
        const updated = createApp(2, '_new');

        appsService.setup(x => x.putApp(app2, {}, app2.version))
            .returns(() => of(updated)).verifiable();

        appsState.update(app2, {}).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should update selected app in snapshot when updated', () => {
        const updated = createApp(1, '_new');

        appsService.setup(x => x.putApp(app1, {}, app1.version))
            .returns(() => of(updated)).verifiable();

        appsState.select(app1.name).subscribe();
        appsState.update(app1, {}).subscribe();

        expect(appsState.snapshot.apps).toEqual([updated, app2]);
        expect(appsState.snapshot.selectedApp).toEqual(updated);
    });

    it('should update app in snapshot when image uploaded', () => {
        const updated = createApp(2, '_new');

        const file = <File>{};

        appsService.setup(x => x.postAppImage(app2, file, app2.version))
            .returns(() => of(50, 60, updated)).verifiable();

        appsState.uploadImage(app2, file).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should update app in snapshot when image removed', () => {
        const updated = createApp(2, '_new');

        appsService.setup(x => x.deleteAppImage(app2, app2.version))
            .returns(() => of(updated)).verifiable();

        appsState.removeImage(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1, updated]);
    });

    it('should remove app from snapshot when left', () => {
        appsService.setup(x => x.leaveApp(app2))
            .returns(() => of({})).verifiable();

        appsState.leave(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1]);
    });

    it('should remove app from snapshot when archived', () => {
        appsService.setup(x => x.deleteApp(app2))
            .returns(() => of({})).verifiable();

        appsState.delete(app2).subscribe();

        expect(appsState.snapshot.apps).toEqual([app1]);
    });

    it('should remove selected app from snapshot when archived', () => {
        appsService.setup(x => x.deleteApp(app1))
            .returns(() => of({})).verifiable();

        appsService.setup(x => x.getApp(app1.name))
            .returns(() => of(app1));

        appsState.select(app1.name).subscribe();
        appsState.delete(app1).subscribe();

        expect(appsState.snapshot.selectedApp).toBeNull();
    });
});