﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
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
    LoadAppsGuard,
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
        canActivate: [MustBeAuthenticatedGuard, LoadAppsGuard],
        children: [
            {
                path: '',
                loadChildren: () => import('./features/apps/module').then(m => m.SqxFeatureAppsModule),
                canActivate: [UnsetAppGuard]
            },
            {
                path: 'administration',
                loadChildren: () => import('./features/administration/module').then(m => m.SqxFeatureAdministrationModule),
                canActivate: [UnsetAppGuard]
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: () => import('./features/dashboard/module').then(m => m.SqxFeatureDashboardModule)
                    },
                    {
                        path: 'content',
                        loadChildren: () => import('./features/content/module').then(m => m.SqxFeatureContentModule)
                    },
                    {
                        path: 'schemas',
                        loadChildren: () => import('./features/schemas/module').then(m => m.SqxFeatureSchemasModule)
                    },
                    {
                        path: 'assets',
                        loadChildren: () => import('./features/assets/module').then(m => m.SqxFeatureAssetsModule)
                    },
                    {
                        path: 'rules',
                        loadChildren: () => import('./features/rules/module').then(m => m.SqxFeatureRulesModule)
                    },
                    {
                        path: 'settings',
                        loadChildren: () => import('./features/settings/module').then(m => m.SqxFeatureSettingsModule)
                    },
                    {
                        path: 'api',
                        loadChildren: () => import('./features/api/module').then(m => m.SqxFeatureApiModule)
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