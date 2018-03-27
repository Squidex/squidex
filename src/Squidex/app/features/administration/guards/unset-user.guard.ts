/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { UsersState } from './../state/users.state';

@Injectable()
export class UnsetUserGuard implements CanActivate {
    constructor(
        private readonly usersState: UsersState
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.usersState.selectUser(null).map(u => u === null);
    }
}