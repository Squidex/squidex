/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AdministrationAreaComponent, ClusterPageComponent, EventConsumerComponent, EventConsumersPageComponent, EventConsumersService, EventConsumersState, RestorePageComponent, UserComponent, UserMustExistGuard, UserPageComponent, UsersPageComponent, UsersService, UsersState } from './declarations';

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
                        component: EventConsumersPageComponent,
                    },
                    {
                        path: 'cluster',
                        component: ClusterPageComponent,
                    },
                    {
                        path: 'restore',
                        component: RestorePageComponent,
                    },
                    {
                        path: 'users',
                        component: UsersPageComponent,
                        children: [
                            {
                                path: ':userId',
                                component: UserPageComponent,
                                canActivate: [UserMustExistGuard],
                            },
                        ],
                    },
                ],
            },
        ],
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        AdministrationAreaComponent,
        ClusterPageComponent,
        EventConsumerComponent,
        EventConsumersPageComponent,
        RestorePageComponent,
        UserComponent,
        UserPageComponent,
        UsersPageComponent,
    ],
    providers: [
        EventConsumersService,
        EventConsumersState,
        UserMustExistGuard,
        UsersService,
        UsersState,
    ],
})
export class SqxFeatureAdministrationModule {}
