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

    const newApp = createApp(3);

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
        const request = { ...newApp };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp)).verifiable();

        appsState.create(request).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([app1, app2, newApp]);
    });

    it('should remove app from snapshot when archived', () => {
        const request = { ...newApp };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp)).verifiable();

        appsService.setup(x => x.deleteApp(newApp))
            .returns(() => of({})).verifiable();

        appsState.create(request).subscribe();

        const appsAfterCreate = appsState.snapshot.apps.values;

        appsState.delete(newApp).subscribe();

        const appsAfterDelete = appsState.snapshot.apps.values;

        expect(appsAfterCreate).toEqual([app1, app2, newApp]);
        expect(appsAfterDelete).toEqual([app1, app2]);
    });

    it('should remove selected app from snapshot when archived', () => {
        const request = { ...newApp };

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp)).verifiable();

        appsService.setup(x => x.deleteApp(newApp))
            .returns(() => of({})).verifiable();

        appsState.create(request).subscribe();
        appsState.select(newApp.name).subscribe();
        appsState.delete(newApp).subscribe();

        expect(appsState.snapshot.selectedApp).toBeNull();
    });
});