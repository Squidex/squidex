/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AppsStoreService } from 'shared';

import { AppMustExistGuard } from './app-must-exist.guard';
import { RouterMockup } from './router-mockup';

describe('AppMustExistGuard', () => {
    let appsStore: IMock<AppsStoreService>;

    beforeEach(() => {
        appsStore = Mock.ofType(AppsStoreService);
    });

    it('should navigate to 404 page if app is not found', (done) => {
        appsStore.setup(x => x.selectApp('my-app'))
            .returns(() => Observable.of(false));
        const router = new RouterMockup();
        const route = <any> { params: { appName: 'my-app' } };

        const guard = new AppMustExistGuard(appsStore.object, <any>router);

        guard.canActivate(route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if app loading fails', (done) => {
        appsStore.setup(x => x.selectApp('my-app'))
            .returns(() => Observable.throw('error'));
        const router = new RouterMockup();
        const route = <any> { params: { appName: 'my-app' } };

        const guard = new AppMustExistGuard(appsStore.object, <any>router);

        guard.canActivate(route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return true if app is found', (done) => {
        appsStore.setup(x => x.selectApp('my-app'))
            .returns(() => Observable.of(true));
        const router = new RouterMockup();
        const route = <any> { params: { appName: 'my-app' } };

        const guard = new AppMustExistGuard(appsStore.object, <any>router);

        guard.canActivate(route, <any>{})
            .subscribe(result => {
                expect(result).toBeTruthy();
                expect(router.lastNavigation).toBeUndefined();

                done();
            });
    });
});