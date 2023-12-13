/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { HelpComponent, HistoryComponent, loadSchemasGuard, schemaMustExistGuard } from '@app/shared';
import { SchemaPageComponent } from './pages/schema/schema-page.component';
import { SchemasPageComponent } from './pages/schemas/schemas-page.component';

export const SCHEMAS_ROUTES: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        canActivate: [loadSchemasGuard],
        children: [
            {
                path: ':schemaName',
                component: SchemaPageComponent,
                canActivate: [schemaMustExistGuard],
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/schemas',
                        },
                    },
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'schemas.{schemaId}',
                        },
                    },
                ],
            },
        ],
    },
];