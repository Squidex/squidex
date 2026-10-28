/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { AllAppsState, AppsDto, AppsService, DialogService } from '@app/shared/internal';
import { createApp } from '../services/apps.service.spec';

describe('AllAppsState', () => {
    const app1 = createApp(1);
    const app2 = createApp(2);

    let dialogs: IMock<DialogService>;
    let appsService: IMock<AppsService>;
    let allAppsState: AllAppsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsService = Mock.ofType<AppsService>();
        allAppsState = new AllAppsState(appsService.object, dialogs.object);
    });

    afterEach(() => {
        appsService.verifyAll();
    });

    it('should load apps', () => {
        appsService.setup(x => x.getAllApps(30, 0, undefined))
            .returns(() => of(new AppsDto({ items: [app1, app2], total: 200, containsAllApps: true, _links: {} }))).verifiable();

        allAppsState.load().subscribe();

        expect(allAppsState.snapshot.apps).toEqual([app1, app2]);
        expect(allAppsState.snapshot.containsAllApps).toBeTruthy();
        expect(allAppsState.snapshot.total).toEqual(200);
        expect(allAppsState.snapshot.isLoaded).toBeTruthy();
        expect(allAppsState.snapshot.isLoading).toBeFalsy();
    });

    it('should reset loading state if loading failed', () => {
        appsService.setup(x => x.getAllApps(30, 0, undefined))
            .returns(() => throwError(() => 'Service Error'));

        allAppsState.load().pipe(onErrorResumeNextWith()).subscribe();

        expect(allAppsState.snapshot.isLoading).toBeFalsy();
    });

    it('should load with query if searching', () => {
        appsService.setup(x => x.getAllApps(30, 0, 'my-query'))
            .returns(() => of(new AppsDto({ items: [app1], total: 1, containsAllApps: true, _links: {} }))).verifiable();

        allAppsState.search(' my-query ').subscribe();

        expect(allAppsState.snapshot.apps).toEqual([app1]);
        expect(allAppsState.snapshot.query).toEqual('my-query');
    });

    it('should load with new pagination if paging', () => {
        appsService.setup(x => x.getAllApps(30, 30, undefined))
            .returns(() => of(new AppsDto({ items: [app2], total: 31, containsAllApps: true, _links: {} }))).verifiable();

        allAppsState.page({ page: 1, pageSize: 30 }).subscribe();

        expect(allAppsState.snapshot.apps).toEqual([app2]);
        expect(allAppsState.snapshot.total).toEqual(31);
    });
});
