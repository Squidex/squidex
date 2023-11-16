/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { allParams } from '@app/framework';
import { RulesState } from '../state/rules.state';

export const ruleMustExistGuard = (route: ActivatedRouteSnapshot) => {
    const rulesState = inject(RulesState);
    const ruleId = allParams(route)['ruleId'];
    const router = inject(Router);

    if (!ruleId || ruleId === 'new') {
        return rulesState.select(null).pipe(map(u => u === null));
    }

    const result =
        rulesState.select(ruleId).pipe(
            tap(rule => {
                if (!rule) {
                    router.navigate(['/404']);
                }
            }),
            map(rule => !!rule));

    return result;
};
