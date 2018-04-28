/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ModuleWithProviders } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';

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

export const routing: ModuleWithProviders = RouterModule.forRoot(routes, { useHash: false, preloadingStrategy: PreloadAllModules });