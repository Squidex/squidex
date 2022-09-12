/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ModuleWithProviders } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppMustExistGuard, LoadAppsGuard, LoadTeamsGuard, MustBeAuthenticatedGuard, MustBeNotAuthenticatedGuard, TeamMustExistGuard, UnsetAppGuard, UnsetTeamGuard } from './shared';
import { AppAreaComponent, ForbiddenPageComponent, HomePageComponent, InternalAreaComponent, LoginPageComponent, LogoutPageComponent, NotFoundPageComponent, TeamsAreaComponent } from './shell';

const routes: Routes = [
    {
        path: '',
        component: HomePageComponent,
        canActivate: [MustBeNotAuthenticatedGuard],
    },
    {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [MustBeAuthenticatedGuard, LoadAppsGuard, LoadTeamsGuard],
        children: [
            {
                path: '',
                loadChildren: () => import('./features/apps/module').then(m => m.SqxFeatureAppsModule),
                canActivate: [UnsetAppGuard, UnsetTeamGuard],
            },
            {
                path: 'administration',
                loadChildren: () => import('./features/administration/module').then(m => m.SqxFeatureAdministrationModule),
                canActivate: [UnsetAppGuard, UnsetTeamGuard],
            },
            {
                path: 'teams',
                component: TeamsAreaComponent,
                canActivate: [UnsetAppGuard, UnsetTeamGuard],
                children: [
                    {
                        path: ':teamName',
                        canActivate: [TeamMustExistGuard],
                        loadChildren: () => import('./features/teams/module').then(m => m.SqxFeatureTeamsModule),
                    },
                ],
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: () => import('./features/dashboard/module').then(m => m.SqxFeatureDashboardModule),
                    },
                    {
                        path: 'content',
                        loadChildren: () => import('./features/content/module').then(m => m.SqxFeatureContentModule),
                    },
                    {
                        path: 'schemas',
                        loadChildren: () => import('./features/schemas/module').then(m => m.SqxFeatureSchemasModule),
                    },
                    {
                        path: 'assets',
                        loadChildren: () => import('./features/assets/module').then(m => m.SqxFeatureAssetsModule),
                    },
                    {
                        path: 'rules',
                        loadChildren: () => import('./features/rules/module').then(m => m.SqxFeatureRulesModule),
                    },
                    {
                        path: 'settings',
                        loadChildren: () => import('./features/settings/module').then(m => m.SqxFeatureSettingsModule),
                    },
                    {
                        path: 'api',
                        loadChildren: () => import('./features/api/module').then(m => m.SqxFeatureApiModule),
                    },
                ],
            },
        ],
    },
    {
        path: 'logout',
        component: LogoutPageComponent,
    },
    {
        path: 'login',
        component: LoginPageComponent,
    },
    {
        path: 'forbidden',
        component: ForbiddenPageComponent,
    },
    {
        path: '**',
        component: NotFoundPageComponent,
    },
];

export const routing: ModuleWithProviders<RouterModule> = RouterModule.forRoot(routes, { useHash: false });
