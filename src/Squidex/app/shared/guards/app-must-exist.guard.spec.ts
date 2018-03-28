/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AppsState } from '@app/shared';

import { AppMustExistGuard } from './app-must-exist.guard';
import { RouterMockup } from './router-mockup';

describe('AppMustExistGuard', () => {
    let appsState: IMock<AppsState>;

    beforeEach(() => {
        appsState = Mock.ofType(AppsState);
    });

    it('should navigate to 404 page if app is not found', (done) => {
        appsState.setup(x => x.selectApp('my-app'))
            .returns(() => Observable.of(null));

        const router = new RouterMockup();
        const route = <any> { params: { appName: 'my-app' } };

        const guard = new AppMustExistGuard(appsState.object, <any>router);

        guard.canActivate(route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return true if app is found', (done) => {
        appsState.setup(x => x.selectApp('my-app'))
            .returns(() => Observable.of(<any>{}));

        const router = new RouterMockup();
        const route = <any> { params: { appName: 'my-app' } };

        const guard = new AppMustExistGuard(appsState.object, <any>router);

        guard.canActivate(route, <any>{})
            .subscribe(result => {
                expect(result).toBeTruthy();
                expect(router.lastNavigation).toBeUndefined();

                done();
            });
    });
});