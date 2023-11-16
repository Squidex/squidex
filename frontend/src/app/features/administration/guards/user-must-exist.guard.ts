/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { allParams } from '@app/shared';
import { UsersState } from '../internal';

export const userMustExistGuard = (route: ActivatedRouteSnapshot) => {
    const usersState = inject(UsersState);

    const userId = allParams(route)['userId'];

    if (!userId || userId === 'new') {
        return usersState.select(null).pipe(map(u => u === null));
    }

    const router = inject(Router);
    const result =
        usersState.select(userId).pipe(
            tap(dto => {
                if (!dto) {
                    router.navigate(['/404']);
                }
            }),
            map(u => !!u));

    return result;
};