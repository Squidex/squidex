/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AdministrationAreaComponent, EventConsumerComponent, EventConsumersPageComponent, EventConsumersService, EventConsumersState, RestorePageComponent, UserComponent, UserMustExistGuard, UserPageComponent, UsersPageComponent, UsersService, UsersState } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AdministrationAreaComponent,
        children: [
            {
                path: '',
                pathMatch: 'full',
                redirectTo: 'users',
            },
            {
                path: 'event-consumers',
                component: EventConsumersPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/admin-consumers',
                        },
                    },
                ],
            },
            {
                path: 'restore',
                component: RestorePageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/admin-restore',
                        },
                    },
                ],
            },
            {
                path: 'users',
                component: UsersPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/admin-users',
                        },
                    },
                    {
                        path: ':userId',
                        component: UserPageComponent,
                        canActivate: [UserMustExistGuard],
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
