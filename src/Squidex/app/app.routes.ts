﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModuleWithProviders } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    AppAreaComponent,
    HomePageComponent,
    InternalAreaComponent,
    LoginPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent
} from './shell';

import {
    AppMustExistGuard,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    UnsetAppGuard
} from './shared';

export const routes: Routes = [
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
                loadChildren: './features/apps/module#SqxFeatureAppsModule',
                canActivate: [UnsetAppGuard]
            },
            {
                path: 'administration',
                loadChildren: './features/administration/module#SqxFeatureAdministrationModule',
                canActivate: [UnsetAppGuard]
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: './features/dashboard/module#SqxFeatureDashboardModule'
                    },
                    {
                        path: 'content',
                        loadChildren: './features/content/module#SqxFeatureContentModule'
                    },
                    {
                        path: 'schemas',
                        loadChildren: './features/schemas/module#SqxFeatureSchemasModule'
                    },
                    {
                        path: 'assets',
                        loadChildren: './features/assets/module#SqxFeatureAssetsModule'
                    },
                    {
                        path: 'rules',
                        loadChildren: './features/rules/module#SqxFeatureRulesModule'
                    },
                    {
                        path: 'settings',
                        loadChildren: './features/settings/module#SqxFeatureSettingsModule'
                    },
                    {
                        path: 'api',
                        loadChildren: './features/api/module#SqxFeatureApiModule'
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
        path: 'login',
        component: LoginPageComponent
    },
    {
        path: '**',
        component: NotFoundPageComponent
    }
];

export const routing: ModuleWithProviders = RouterModule.forRoot(routes, { useHash: false });