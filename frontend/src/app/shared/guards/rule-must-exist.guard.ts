/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { allParams } from '@app/framework';
import { RulesState } from './../state/rules.state';

@Injectable()
export class RuleMustExistGuard implements CanActivate {
    constructor(
        private readonly rulesState: RulesState,
        private readonly router: Router,
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const ruleId = allParams(route)['ruleId'];

        if (!ruleId || ruleId === 'new') {
            return this.rulesState.select(null).pipe(map(u => u === null));
        }

        const result =
            this.rulesState.select(ruleId).pipe(
                tap(rule => {
                    if (!rule) {
                        this.router.navigate(['/404']);
                    }
                }),
                map(rule => !!rule));

        return result;
    }
}
