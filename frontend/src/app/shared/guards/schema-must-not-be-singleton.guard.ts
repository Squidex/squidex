/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { map, take, tap } from 'rxjs/operators';
import { defined } from '@app/framework';
import { SchemasState } from '../state/schemas.state';

export const schemaMustNotBeSingletonGuard = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const schemasState = inject(SchemasState);
    const router = inject(Router);

    const result =
        schemasState.selectedSchema.pipe(
            defined(),
            take(1),
            tap(schema => {
                if (schema.type === 'Singleton') {
                    if (state.url.includes('/new')) {
                        const parentUrl = state.url.slice(0, state.url.indexOf(route.url[route.url.length - 1].path));

                        router.navigate([parentUrl, schema.id]);
                    } else {
                        router.navigate([state.url, schema.id]);
                    }
                }
            }),
            map(schema => schema.type === 'Default'));

    return result;
};
