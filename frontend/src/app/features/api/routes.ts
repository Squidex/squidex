/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { ApiAreaComponent } from './api-area.component';
import { GraphQLPageComponent } from './pages/graphql/graphql-page.component';

export const API_ROUTES: Routes = [
    {
        path: '',
        component: ApiAreaComponent,
        children: [
            {
                path: 'graphql',
                component: GraphQLPageComponent,
            },
        ],
    },
];
