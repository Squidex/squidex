/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock } from 'typemoq';

import {
    AppDto,
    AppsService,
    AppsState,
    DialogService
} from '@app/shared/internal';

import { createApp } from '../services/apps.service.spec';

describe('AppsState', () => {
    const app1 = createApp(1);
    const app2 = createApp(2);

    const newApp1 = createApp(1, 'new');
    const newApp2 = createApp(2, 'new');
    const newApp3 = createApp(3, 'new');

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
        expect(appsState.snapshot.apps.values).toEqual([app1, app2]);
    });

    it('should select app', () => {
        let selectedApp: AppDto;

        appsState.select(app1.name).subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBe(app1);
        expect(appsState.snapshot.selectedApp).toBe(app1);
    });

    it('should return null on select when unselecting user', () => {
        let selectedApp: AppDto;

        appsState.select(null).subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return null on select when apps is not found', () => {
        let selectedApp: AppDto;

        appsState.select('unknown').subscribe(x => {
            selectedApp = x!;
        });

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should add app to snapshot when created', () => {
        const request = { ...newApp3 };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp3)).verifiable();

        appsState.create(request).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([app1, app2, newApp3]);
    });

    it('should update app in snapshot when updated', () => {
        appsService.setup(x => x.putApp(app2, {}, app2.version))
            .returns(() => of(newApp2)).verifiable();

        appsState.select(app1.name).subscribe();
        appsState.update(app2, {}).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([app1, newApp2]);
        expect(appsState.snapshot.selectedApp).toEqual(app1);
    });

    it('should update selected app in snapshot when updated', () => {
        appsService.setup(x => x.putApp(app1, {}, app1.version))
            .returns(() => of(newApp1)).verifiable();

        appsState.select(app1.name).subscribe();
        appsState.update(app1, {}).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([newApp1, app2]);
        expect(appsState.snapshot.selectedApp).toEqual(newApp1);
    });

    it('should remove app from snapshot when archived', () => {
        appsService.setup(x => x.deleteApp(app2))
            .returns(() => of({})).verifiable();

        appsState.select(app1.name).subscribe();
        appsState.delete(app2).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([app1]);
        expect(appsState.snapshot.selectedApp).toEqual(app1);
    });

    it('should remove selected app from snapshot when archived', () => {
        appsService.setup(x => x.deleteApp(app1))
            .returns(() => of({})).verifiable();

        appsState.select(app1.name).subscribe();
        appsState.delete(app1).subscribe();

        expect(appsState.snapshot.selectedApp).toBeNull();
    });
});