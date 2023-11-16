/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AppsState, UIState } from '@app/shared/internal';
import { appMustExistGuard } from './app-must-exist.guard';

describe('AppMustExistGuard', () => {
    const route: any = {
        params: {
            appName: 'my-app',
        },
    };

    let router: IMock<Router>;
    let appsState: IMock<AppsState>;
    let uiState: IMock<UIState>;

    beforeEach(() => {
        uiState = Mock.ofType<UIState>();
        router = Mock.ofType<Router>();
        appsState = Mock.ofType<AppsState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: AppsState,
                    useValue: appsState.object,
                },
                {
                    provide: Router,
                    useValue: router.object,
                },
                {
                    provide: UIState,
                    useValue: uiState.object,
                },
            ],
        });
    });

    bit('should navigate to 404 page if app is not found', async () => {
        appsState.setup(x => x.select('my-app'))
            .returns(() => of(null));

        const result = await firstValueFrom(appMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should return true if app is found', async () => {
        uiState.setup(x => x.loadApp('my-app'))
            .returns(() => of(<any>{}));

        appsState.setup(x => x.select('my-app'))
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(appMustExistGuard(route));

        expect(result).toBeTruthy();
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}