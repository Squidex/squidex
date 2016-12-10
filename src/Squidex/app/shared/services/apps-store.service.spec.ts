/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as TypeMoq from 'typemoq';

import { Observable } from 'rxjs';

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

    const oldApps = [new AppDto('id', 'old-name', now, now, 'Owner')];
    const newApp =   new AppDto('id', 'new-name', now, now, 'Owner');

    let appsService: TypeMoq.Mock<AppsService>;
    let authService: TypeMoq.Mock<AuthService>;

    beforeEach(() => {
        appsService = TypeMoq.Mock.ofType(AppsService);
        authService = TypeMoq.Mock.ofType(AuthService);
    });

    it('should load when authenticated once', () => {
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];
        let result2: AppDto[];

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

    it('should reload value from apps-service when called', () => {
         authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.exactly(2));

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];
        let result2: AppDto[];

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.reload();

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
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.postApp(TypeMoq.It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result1: AppDto[];
        let result2: AppDto[];

        store.apps.subscribe(x => {
            result1 = x;
        }).unsubscribe();

        store.createApp(new CreateAppDto('new-name'), now).subscribe(x => { });

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
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.postApp(TypeMoq.It.isAny()))
            .returns(() => Observable.of(newApp))
            .verifiable(TypeMoq.Times.once());

        const store = new AppsStoreService(authService.object, appsService.object);

        let result: AppDto[] = null;

        store.createApp(new CreateAppDto('new-name'), now).subscribe(x => { });

        store.apps.subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result).toBeNull();

        appsService.verifyAll();
    });

    it('should select app', (done) => {
        authService.setup(x => x.isAuthenticated)
            .returns(() => Observable.of(true))
            .verifiable(TypeMoq.Times.once());

        appsService.setup(x => x.getApps())
            .returns(() => Observable.of(oldApps))
            .verifiable(TypeMoq.Times.once());

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