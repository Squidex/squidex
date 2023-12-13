/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { DashboardPageComponent } from './pages/dashboard-page.component';

export const DASHBOARD_ROUTES: Routes = [
    {
        path: '',
        component: DashboardPageComponent,
    },
];