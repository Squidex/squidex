/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { SchemasState } from '../state/schemas.state';

export const schemaMustNotBeSingletonGuard = (forExtension: boolean) => {
    return (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
        const schemasState = inject(SchemasState);
        const router = inject(Router);

        const schema = schemasState.selectedSchema();
        if (!schema) {
            return false;
        }

        if (schema.type === 'Singleton') {
            if (state.url.includes('/new')) {
                const parentUrl = state.url.slice(0, state.url.indexOf(route.url[route.url.length - 1].path));

                router.navigate([parentUrl, schema.id]);
            } else {
                router.navigate([state.url, schema.id]);
            }
        } else if (!!schema.properties.contentsListUrl !== forExtension) {
            if (forExtension) {
                router.navigate([state.url, '..']);
            } else {
                router.navigate([state.url, 'extension']);
            }
        }

        return schema.type === 'Default' && !!schema.properties.contentsListUrl === forExtension;
    };
};
