/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { allParams } from '@app/framework';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router,
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const schemaName = allParams(route)['schemaName'];

        const result =
            this.schemasState.select(schemaName).pipe(
                tap(schema => {
                    if (!schema) {
                        this.router.navigate(['/404']);
                    }
                }),
                map(schema => !!schema));

        return result;
    }
}
