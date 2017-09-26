/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';
import { It, IMock, Mock, Times } from 'typemoq';

import {
    AppDto,
    AppsService,
    AppsStoreService,
    CreateAppDto,
    DateTime
} from './../';

describe('AppsStoreService', () => {
    const now = DateTime.now();

    const oldApps = [new AppDto('id', 'old-name', 'Owner', now, now)];
    const newApp =   new AppDto('id', 'new-name', 'Owner', now, now);

    let appsService: IMock<AppsService>;

    beforeEach(() => {
        appsService = Mock.ofType(AppsService);
    });

    it('should load automatically', () => {
        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        const store = new AppsStoreService(appsService.object);

        let result1: AppDto[] | null = null;
        let result2: AppDto[] | null = null;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(result2).toEqual(oldApps);

        appsService.verifyAll();
    });

    it('should add app to cache when created', () => {
        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        appsService.setup(x => x.postApp(It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        const store = new AppsStoreService(appsService.object);

        let result1: AppDto[] | null = null;
        let result2: AppDto[] | null = null;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(new CreateAppDto('new-name'), now).subscribe();

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(JSON.stringify(result2)).toEqual(JSON.stringify(oldApps.concat([newApp])));

        appsService.verifyAll();
    });

    it('should select app', (done) => {
        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        const store = new AppsStoreService(appsService.object);

        store.selectApp('old-name').subscribe(isSelected => {
            expect(isSelected).toBeTruthy();

            appsService.verifyAll();

            done();
        }, err => {
            expect(err).toBeNull();

            done();
        });
    });
});