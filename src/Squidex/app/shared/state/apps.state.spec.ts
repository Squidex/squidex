/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    AppDto,
    AppsService,
    AppsState,
    CreateAppDto,
    DateTime
} from './../';

describe('AppsState', () => {
    const now = DateTime.now();

    const oldApps = [
        new AppDto('id1', 'old-name1', 'Owner', now, now, 'Free', 'Plan'),
        new AppDto('id2', 'old-name2', 'Owner', now, now, 'Free', 'Plan')
    ];
    const newApp = new AppDto('id3', 'new-name', 'Owner', now, now, 'Free', 'Plan');

    let appsService: IMock<AppsService>;
    let appsState: AppsState;

    beforeEach(() => {
        appsService = Mock.ofType(AppsService);

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        appsState = new AppsState(appsService.object);
        appsState.loadApps().subscribe();
    });

    it('should load apps', () => {
        expect(appsState.snapshot.apps.values).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should add app to state when created', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        appsState.createApp(request, now).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([newApp, ...oldApps]);

        appsService.verifyAll();
    });

    it('should remove app from state when archived', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        appsService.setup(x => x.deleteApp(newApp.name))
            .returns(() => Observable.of({}))
            .verifiable(Times.once());

        appsState.createApp(request, now).subscribe();

        const appsAfterCreate = appsState.snapshot.apps.values;

        appsState.deleteApp(newApp.name).subscribe();

        const appsAfterDelete = appsState.snapshot.apps.values;

        expect(appsAfterCreate).toEqual([newApp, ...oldApps]);
        expect(appsAfterDelete).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should select app', () => {
        let selectedApp: AppDto;

        appsState.selectApp(oldApps[0].name).subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBe(oldApps[0]);
        expect(appsState.snapshot.selectedApp).toBe(oldApps[0]);
    });

    it('should return null when unselecting app', () => {
        let selectedApp: AppDto;

        appsState.selectApp(null).subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return null when app to select is not found', () => {
        let selectedApp: AppDto;

        appsState.selectApp('unknown').subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });
});