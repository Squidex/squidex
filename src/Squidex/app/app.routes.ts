/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import {
    AppsPageComponent,
    LoginPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent
} from './components';

import { 
    AuthGuard
} from './shared';

export const routes: Ng2Router.Routes = [
    {
        path: '',
        redirectTo: 'apps',
        pathMatch: 'full'
    },
    {
        path: 'apps',
        component: AppsPageComponent,
        canActivate: [AuthGuard]
    },
    {
        path: 'login',
        component: LoginPageComponent
    },
    {
        path: 'logout',
        component: LogoutPageComponent
    },
    {
        path: '404',
        component: NotFoundPageComponent
    },
    {
        path: '**',
        component: NotFoundPageComponent
    }
];

export const routing: Ng2.ModuleWithProviders = Ng2Router.RouterModule.forRoot(routes, { useHash: false, enableTracing: true });