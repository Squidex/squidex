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
    AuthService,
    CreateAppDto,
    DateTime
} from './../';

describe('AppsStoreService', () => {
    const now = DateTime.now();

    const oldApps = [new AppDto('id', 'old-name', 'Owner', now, now)];
    const newApp =   new AppDto('id', 'new-name', 'Owner', now, now);

    let appsService: IMock<AppsService>;
    let authService: IMock<AuthService>;

    beforeEach(() => {
        appsService = Mock.ofType(AppsService);
        authService = Mock.ofType(AuthService);
    });

    it('should load when authenticated once', () => {
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

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
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        appsService.setup(x => x.postApp(It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[] | null = null;
        let result2: AppDto[] | null = null;

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(new CreateAppDto('new-name'), now).subscribe(x => { /* Do Nothing */ });

        store.apps.subscribe(x => {
            result2 = x;
        }).unsubscribe();

        expect(result1).toEqual(oldApps);
        expect(JSON.stringify(result2)).toEqual(JSON.stringify(oldApps.concat([newApp])));

        appsService.verifyAll();
    });

    it('should not add app to cache when cache is null', () => {
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(false))
            .verifiable(Times.once());

        appsService.setup(x => x.postApp(It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result: AppDto[] | null = null;

        store.createApp(new CreateAppDto('new-name'), now).subscribe(x => { /* Do Nothing */ });

        store.apps.subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result).toBeNull();

        appsService.verifyAll();
    });

    it('should select app', (done) => {
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        store.selectApp('old-name').then((isSelected) => {
            expect(isSelected).toBeTruthy();

            appsService.verifyAll();

            done();
        }, err => {
            expect(err).toBeNull();

            done();
        });
    });
});