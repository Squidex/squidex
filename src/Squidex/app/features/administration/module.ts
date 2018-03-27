/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    SqxSharedModule,
    SqxFrameworkModule
} from 'shared';

import {
    AdministrationAreaComponent,
    EventConsumersPageComponent,
    EventConsumersService,
    UserPageComponent,
    UsersPageComponent,
    UsersService
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
                                component: UserPageComponent
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
        SqxSharedModule,
        SqxFrameworkModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AdministrationAreaComponent,
        EventConsumersPageComponent,
        UserPageComponent,
        UsersPageComponent
    ],
    providers: [
        EventConsumersService,
        UnsetUserGuard,
        UserMustExistGuard,
        UsersService
    ]
})
export class SqxFeatureAdministrationModule { }