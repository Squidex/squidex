/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    ResolveUserGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    AdministrationAreaComponent,
    EventConsumersPageComponent,
    UserPageComponent,
    UsersPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AdministrationAreaComponent,
        children: [
            {
                path: '',
                children: [
                    {
                        path: 'event-consumers',
                        component: EventConsumersPageComponent
                    },
                    {
                        path: 'users',
                        component: UsersPageComponent,
                        children: [
                            {
                                path: 'new',
                                component: UserPageComponent
                            },
                            {
                                path: ':userId',
                                component: UserPageComponent,
                                resolve: {
                                    user: ResolveUserGuard
                                }
                            }
                        ]
                    }
                ]
            }
        ]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AdministrationAreaComponent,
        EventConsumersPageComponent,
        UserPageComponent,
        UsersPageComponent
    ]
})
export class SqxFeatureAdministrationModule { }