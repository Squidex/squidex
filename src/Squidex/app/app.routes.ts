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
import { SqxFeatureContentModule } from './features/content';
import { SqxFeatureDashboardModule } from './features/dashboard';
import { SqxFeatureMediaModule } from './features/media';
import { SqxFeatureSchemasModule } from './features/schemas';
import { SqxFeatureSettingsModule } from './features/settings';

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
                        loadChildren: () => SqxFeatureDashboardModule
                    }, {
                        path: 'content',
                        loadChildren: () => SqxFeatureContentModule
                    }, {
                        path: 'media',
                        loadChildren: () => SqxFeatureMediaModule
                    }, {
                        path: 'schemas',
                        loadChildren: () => SqxFeatureSchemasModule
                    }, {
                        path: 'settings',
                        loadChildren: () => SqxFeatureSettingsModule
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