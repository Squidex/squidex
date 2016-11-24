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
    ClientsPageComponent,
    ContributorsPageComponent,
    DashboardPageComponent,
    InternalAreaComponent,
    HomePageComponent,
    LanguagesPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent,
    SchemasPageComponent
} from './components';

import {
    AppMustExistGuard,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard
} from './shared';

export const routes: Ng2Router.Routes = [
    {
        path: '',
        component: HomePageComponent,
        canActivate: [MustBeNotAuthenticatedGuard]
    },
    {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [MustBeAuthenticatedGuard],
        children: [
            {
                path: '',
                component: AppsPageComponent
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        redirectTo: 'dashboard'
                    },
                    {
                        path: 'dashboard',
                        component: DashboardPageComponent
                    },
                    {
                        path: 'schemas',
                        component: SchemasPageComponent
                    },
                    {
                        path: 'contributors',
                        component: ContributorsPageComponent
                    },
                    {
                        path: 'clients',
                        component: ClientsPageComponent
                    },
                    {
                        path: 'languages',
                        component: LanguagesPageComponent
                    }
                ]
            }
        ]
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