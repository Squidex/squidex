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
import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistPublishedGuard implements CanActivate {
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
                    if (!schema || !schema.isPublished || schema.type === 'Component') {
                        this.router.navigate(['/404']);
                    }
                }),
                map(schema => schema?.isPublished === true && schema.type !== 'Component'));

        return result;
    }
}
