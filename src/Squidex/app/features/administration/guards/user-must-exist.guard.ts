/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from 'framework';

import { UsersState } from './../state/users.state';

@Injectable()
export class UserMustExistGuard implements CanActivate {
    constructor(
        private readonly usersState: UsersState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const params = allParams(route);

        const userId = params['userId'];

        if (!userId) {
            throw 'Route must contain user id.';
        }

        const result =
            this.usersState.selectUser(userId).map(u => !!u)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .catch(error => {
                    this.router.navigate(['/404']);

                    return Observable.of(false);
                });

        return result;
    }
}