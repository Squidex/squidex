/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { appMustExistGuard, loadAppsGuard, loadSettingsGuard, LoadTeamsGuard, mustBeAuthenticatedGuard, mustBeNotAuthenticatedGuard, TeamMustExistGuard, unsetAppGuard, unsetTeamGuard } from './shared';
import { AppAreaComponent, ForbiddenPageComponent, HomePageComponent, InternalAreaComponent, LoginPageComponent, LogoutPageComponent, NotFoundPageComponent, TeamsAreaComponent } from './shell';

export const APP_ROUTES: Routes = [
    {
        path: '',
        component: HomePageComponent,
        canActivate: [mustBeNotAuthenticatedGuard],
    },
    {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [mustBeAuthenticatedGuard, loadAppsGuard, LoadTeamsGuard, loadSettingsGuard],
        children: [
            {
                path: '',
                loadChildren: () => import('./features/apps/module').then(m => m.APPS_ROUTES),
                canActivate: [unsetAppGuard, unsetTeamGuard],
            },
            {
                path: 'administration',
                loadChildren: () => import('./features/administration/module').then(m => m.ADMINISTRATION_ROUTES),
                canActivate: [unsetAppGuard, unsetTeamGuard],
            },
            {
                path: 'teams',
                component: TeamsAreaComponent,
                canActivate: [unsetAppGuard, unsetTeamGuard],
                children: [
                    {
                        path: ':teamName',
                        canActivate: [TeamMustExistGuard],
                        loadChildren: () => import('./features/teams/module').then(m => m.TEAM_ROUTES),
                    },
                ],
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [appMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: () => import('./features/dashboard/module').then(m => m.DASHBOARD_ROUTES),
                    },
                    {
                        path: 'content',
                        loadChildren: () => import('./features/content/module').then(m => m.CONTENT_ROUTES),
                    },
                    {
                        path: 'schemas',
                        loadChildren: () => import('./features/schemas/module').then(m => m.SCHEMAS_ROUTES),
                    },
                    {
                        path: 'assets',
                        loadChildren: () => import('./features/assets/module').then(m => m.ASSETS_ROUTES),
                    },
                    {
                        path: 'rules',
                        loadChildren: () => import('./features/rules/module').then(m => m.RULES_ROUTES),
                    },
                    {
                        path: 'settings',
                        loadChildren: () => import('./features/settings/module').then(m => m.SETTINGS_ROUTES),
                    },
                    {
                        path: 'api',
                        loadChildren: () => import('./features/api/module').then(m => m.API_ROUTES),
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
