/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, switchMap, tap } from 'rxjs/operators';
import { AppsState } from './../state/apps.state';
import { UIState } from '../internal';

@Injectable()
export class AppMustExistGuard  {
    constructor(
        private readonly appsState: AppsState,
        private readonly router: Router,
        private readonly uiState: UIState,
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const appName = route.params['appName'];

        const result =
            this.appsState.select(appName).pipe(
                tap(app => {
                    if (!app) {
                        this.router.navigate(['/404']);
                    }
                }),
                switchMap(app => {
                    if (app) {
                        return this.uiState.loadApp(appName).pipe(map(() => app));
                    } else {
                        return of(app);
                    }
                }),
                map(app => !!app));

        return result;
    }
}
