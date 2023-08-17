/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppsState, UIState } from '@app/shared/internal';
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
    let uiState: IMock<UIState>;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();
        router = Mock.ofType<Router>();
        appsState = Mock.ofType<AppsState>();
        appGuard = new AppMustExistGuard(appsState.object, router.object, uiState.object);
    });

    it('should navigate to 404 page if app is not found', async () => {
        appsState.setup(x => x.select('my-app'))
            .returns(() => of(null));

        const result = await firstValueFrom(appGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should return true if app is found', async () => {
        uiState.setup(x => x.loadApp('my-app'))
            .returns(() => of(<any>{}));

        appsState.setup(x => x.select('my-app'))
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(appGuard.canActivate(route));

        expect(result).toBeTruthy();
    });
});
