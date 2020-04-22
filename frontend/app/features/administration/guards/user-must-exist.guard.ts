/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { UsersState } from '@app/features/administration/internal';
import { allParams } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

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
            this.usersState.select(userId).pipe(
                tap(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                }),
                map(u => !!u));

        return result;
    }
}