/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import {
    AppAreaComponent,
    HomePageComponent,
    InternalAreaComponent,
    LogoutPageComponent,
    NotFoundPageComponent
} from './shell';

import {
    AppMustExistGuard,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard
} from './shared';

import { SqxFeatureAppsModule } from './features/apps';

export const routes: Ng2Router.Routes = [
    {
        path: '',
        component: HomePageComponent,
        canActivate: [MustBeNotAuthenticatedGuard]
    }, {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [MustBeAuthenticatedGuard],
        children: [
            {
                path: '',
                loadChildren: () => SqxFeatureAppsModule
            }, {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: './features/dashboard/module#SqxFeatureDashboardModule'
                    }, {
                        path: 'content',
                        loadChildren: './features/content/module#SqxFeatureContentModule'
                    }, {
                        path: 'media',
                        loadChildren: './features/media/module#SqxFeatureMediaModule'
                    }, {
                        path: 'schemas',
                        loadChildren: './features/schemas/module#SqxFeatureSchemaModule'
                    }, {
                        path: 'settings',
                        loadChildren: './features/settings/module#SqxFeatureSettingsModule'
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

export const routing: Ng2.ModuleWithProviders = Ng2Router.RouterModule.forRoot(routes, { useHash: false });