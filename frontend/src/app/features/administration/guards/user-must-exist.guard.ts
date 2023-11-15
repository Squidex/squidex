/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { allParams } from '@app/shared';
import { UsersState } from '../internal';

@Injectable()
export class UserMustExistGuard  {
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
