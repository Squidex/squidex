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

import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const params = allParams(route);

        const schemaName = params['schemaName'];

        if (!schemaName) {
            throw 'Route must contain schema name.';
        }

        const result =
            this.schemasState.selectSchema(schemaName)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .map(u => u !== null);

        return result;
    }
}