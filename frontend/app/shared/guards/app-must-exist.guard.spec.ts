/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { AppsState } from '@app/shared';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppMustExistGuard } from './app-must-exist.guard';

describe('AppMustExistGuard', () => {
    const route: any = {
        params: {
            appName: 'my-app',
        },
    };

    let router: IMock<Router>;

    let appsState: IMock<AppsState>;
    let appGuard: AppMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();

        appsState = Mock.ofType<AppsState>();
        appGuard = new AppMustExistGuard(appsState.object, router.object);
    });

    it('should navigate to 404 page if app is not found', () => {
        appsState.setup(x => x.select('my-app'))
            .returns(() => of(null));

        let result: boolean;

        appGuard.canActivate(route).subscribe(x => {
            result = x;
        });

        expect(result!).toBeFalsy();

        appsState.verify(x => x.select('my-app'), Times.once());
    });

    it('should return true if app is found', () => {
        appsState.setup(x => x.select('my-app'))
            .returns(() => of(<any>{}));

        let result: boolean;

        appGuard.canActivate(route).subscribe(x => {
            result = x;
        });

        expect(result!).toBeTruthy();

        // router.verify(x => x.navigate(['/404']), Times.once());
    });
});
