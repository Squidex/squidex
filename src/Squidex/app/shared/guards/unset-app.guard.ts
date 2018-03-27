/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { AppsState } from './../state/apps.state';

@Injectable()
export class UnsetAppGuard implements CanActivate {
    constructor(
        private readonly appsState: AppsState
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.appsState.selectApp(null).map(a => a === null);
    }
}