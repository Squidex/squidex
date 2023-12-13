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
import { SchemasState } from '../state/schemas.state';

export const schemaMustExistPublishedGuard = (route: ActivatedRouteSnapshot) => {
    const schemasState = inject(SchemasState);
    const schemaName = allParams(route)['schemaName'];
    const router = inject(Router);

    const result =
        schemasState.select(schemaName).pipe(
            tap(schema => {
                if (!schema || !schema.isPublished || schema.type === 'Component') {
                    router.navigate(['/404']);
                }
            }),
            map(schema => schema?.isPublished === true && schema.type !== 'Component'));

    return result;
};
