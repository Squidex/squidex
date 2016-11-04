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
    AppAreaComponent,
    DashboardComponent,
    InternalAreaComponent,
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
        redirectTo: 'app', pathMatch: 'full'
    },
    {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                component: AppsPageComponent
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                children: [
                    {
                        path: '',
                        component: DashboardComponent
                    }
                ]
            }
        ]
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
        path: '**',
        component: NotFoundPageComponent
    }
];

export const routing: Ng2.ModuleWithProviders = Ng2Router.RouterModule.forRoot(routes, { useHash: false, enableTracing: true });