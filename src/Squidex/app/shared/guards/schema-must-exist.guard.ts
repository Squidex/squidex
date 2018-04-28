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

import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const schemaName = allParams(route)['schemaName'];

        const result =
            this.schemasState.select(schemaName)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .map(s => s !== null);

        return result;
    }
}