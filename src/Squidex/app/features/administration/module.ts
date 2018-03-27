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
    UnsetUserGuard,
    UserPageComponent,
    UserMustExistGuard,
    UsersPageComponent,
    UsersService,
    UsersState
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
                                component: UserPageComponent,
                                canActivate: [UnsetUserGuard]
                            },
                            {
                                path: ':userId',
                                component: UserPageComponent,
                                canActivate: [UserMustExistGuard]
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
        UsersService,
        UsersState
    ]
})
export class SqxFeatureAdministrationModule { }