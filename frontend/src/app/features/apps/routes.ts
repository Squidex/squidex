/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { AppsPageComponent } from './pages/apps-page.component';

export const APPS_ROUTES: Routes = [
    {
        path: '',
        component: AppsPageComponent,
    },
];
