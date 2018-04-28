/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from '@app/framework';

import { UsersState } from './../state/users.state';

@Injectable()
export class UserMustExistGuard implements CanActivate {
    constructor(
        private readonly usersState: UsersState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const userId = allParams(route)['userId'];

        const result =
            this.usersState.select(userId)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .map(u => u !== null);

        return result;
    }
}