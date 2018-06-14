/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    AppDto,
    AppsService,
    AppsState,
    CreateAppDto,
    DateTime,
    DialogService
} from './../';

describe('AppsState', () => {
    const now = DateTime.now();

    const oldApps = [
        new AppDto('id1', 'old-name1', 'Owner', now, now, 'Free', 'Plan'),
        new AppDto('id2', 'old-name2', 'Owner', now, now, 'Free', 'Plan')
    ];

    const newApp = new AppDto('id3', 'new-name', 'Owner', now, now, 'Free', 'Plan');

    let dialogs: IMock<DialogService>;
    let appsService: IMock<AppsService>;
    let appsState: AppsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsService = Mock.ofType<AppsService>();

        appsService.setup(x => x.getApps())
            .returns(() => of(oldApps))
            .verifiable(Times.once());

        appsState = new AppsState(appsService.object, dialogs.object);
        appsState.load().subscribe();
    });

    it('should load apps', () => {
        expect(appsState.snapshot.apps.values).toEqual(oldApps);
    });

    it('should select app', () => {
        let selectedApp: AppDto;

        appsState.select(oldApps[0].name).subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBe(oldApps[0]);
        expect(appsState.snapshot.selectedApp).toBe(oldApps[0]);
    });

    it('should return null on select when unselecting user', () => {
        let selectedApp: AppDto;

        appsState.select(null).subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should return null on select when apps is not found', () => {
        let selectedApp: AppDto;

        appsState.select('unknown').subscribe(x => {
            selectedApp = x!;
        }).unsubscribe();

        expect(selectedApp!).toBeNull();
        expect(appsState.snapshot.selectedApp).toBeNull();
    });

    it('should add app to snapshot when created', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp));

        appsState.create(request, now).subscribe();

        expect(appsState.snapshot.apps.values).toEqual([newApp, ...oldApps]);
    });

    it('should remove app from snashot when archived', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => of(newApp));

        appsService.setup(x => x.deleteApp(newApp.name))
            .returns(() => of({}));

        appsState.create(request, now).subscribe();

        const appsAfterCreate = appsState.snapshot.apps.values;

        appsState.delete(newApp.name).subscribe();

        const appsAfterDelete = appsState.snapshot.apps.values;

        expect(appsAfterCreate).toEqual([newApp, ...oldApps]);
        expect(appsAfterDelete).toEqual(oldApps);
    });
});