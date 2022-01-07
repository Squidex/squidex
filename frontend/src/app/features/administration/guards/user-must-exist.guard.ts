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
import { Observable, map, tap } from 'rxjs';

@Injectable()
export class UserMustExistGuard implements CanActivate {
    constructor(
        private readonly usersState: UsersState,
        private readonly router: Router,
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const userId = allParams(route)['userId'];

        if (!userId || userId === 'new') {
            return this.usersState.select(null).pipe(map(u => u === null));
        }

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
