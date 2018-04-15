/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from '@app/framework';

import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistPublishedGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const schemaName = allParams(route)['schemaName'];

        const result =
            this.schemasState.selectSchema(schemaName)
                .do(dto => {
                    if (!dto || !dto.isPublished) {
                        this.router.navigate(['/404']);
                    }
                })
                .map(s => s !== null && s.isPublished);

        return result;
    }
}