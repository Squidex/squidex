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
    DateTime,
    ImmutableArray
} from './../';

describe('AppsState', () => {
    const now = DateTime.now();

    const oldApps = [
        new AppDto('id1', 'old-name1', 'Owner', now, now, 'Free', 'Plan'),
        new AppDto('id2', 'old-name2', 'Owner', now, now, 'Free', 'Plan')
    ];
    const newApp = new AppDto('id3', 'new-name', 'Owner', now, now, 'Free', 'Plan');

    let appsService: IMock<AppsService>;

    beforeEach(() => {
        appsService = Mock.ofType(AppsService);

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());
    });

    it('should load automatically', () => {
        const store = new AppsState(appsService.object);

        let result1: ImmutableArray<AppDto>;
        let result2: ImmutableArray<AppDto>;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1!.values).toEqual(oldApps);
        expect(result2!.values).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should add app to cache when created', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        const store = new AppsState(appsService.object);

        let result1: ImmutableArray<AppDto>;
        let result2: ImmutableArray<AppDto>;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(request, now).subscribe();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1!.values).toEqual(oldApps);
        expect(result2!.values).toEqual(oldApps.concat([newApp]));

        appsService.verifyAll();
    });

    it('should remove app from cache when archived', () => {
        const request = new CreateAppDto(newApp.name);

        appsService.setup(x => x.postApp(request))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        appsService.setup(x => x.deleteApp(newApp.name))
            .returns(() => Observable.of({}))
            .verifiable(Times.once());

        const store = new AppsState(appsService.object);

        let result1: ImmutableArray<AppDto>;
        let result2: ImmutableArray<AppDto>;
        let result3: ImmutableArray<AppDto>;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(request, now).subscribe();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        store.deleteApp(newApp.name).subscribe();

        store.apps.subscribe(x => {
            result3 = x;
        }).unsubscribe();

        expect(result1!.values).toEqual(oldApps);
        expect(result2!.values).toEqual(oldApps.concat([newApp]));
        expect(result3!.values).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should select app', (done) => {
        const store = new AppsState(appsService.object);

        store.selectApp(oldApps[0].name).subscribe(isSelected => {
            expect(isSelected).toBeTruthy();

            appsService.verifyAll();

            done();
        }, err => {
            expect(err).toBeNull();

            done();
        });
    });
});