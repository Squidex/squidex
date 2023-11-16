/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { AppsState, UIState } from '@app/shared/internal';

export const appMustExistGuard = (route: ActivatedRouteSnapshot) => {
    const appsState = inject(AppsState);
    const appName = route.params['appName'];
    const router = inject(Router);
    const uiState = inject(UIState);

    const result =
        appsState.select(appName).pipe(
            tap(app => {
                if (!app) {
                    router.navigate(['/404']);
                }
            }),
            switchMap(app => {
                if (app) {
                    return uiState.loadApp(appName).pipe(map(() => app));
                } else {
                    return of(app);
                }
            }),
            map(app => !!app));

    return result;
};