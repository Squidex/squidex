/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';

import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustNotBeSingletonGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const result =
            this.schemasState.selectedSchema.pipe(
                take(1),
                tap(schema => {
                    if (schema.isSingleton) {
                        if (state.url.indexOf('/new') >= 0) {
                            const parentUrl = state.url.slice(0, state.url.indexOf(route.url[route.url.length - 1].path));

                            this.router.navigate([parentUrl, schema.id]);
                        } else {
                            this.router.navigate([state.url, schema.id]);
                        }
                    }
                }),
                map(schema => !schema.isSingleton));

        return result;
    }
}