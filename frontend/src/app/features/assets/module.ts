/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { AssetsFiltersPageComponent } from './pages/assets-filters-page.component';
import { AssetsPageComponent } from './pages/assets-page.component';

export const ASSETS_ROUTES: Routes = [
    {
        path: '',
        component: AssetsPageComponent,
        children: [
            {
                path: 'filters',
                component: AssetsFiltersPageComponent,
            },
        ],
    },
];