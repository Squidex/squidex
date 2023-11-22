/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { HelpComponent } from '@app/shared';
import { AdministrationAreaComponent } from './administration-area.component';
import { userMustExistGuard } from './guards/user-must-exist.guard';
import { EventConsumersService, EventConsumersState, UsersService, UsersState } from './internal';
import { EventConsumersPageComponent } from './pages/event-consumers/event-consumers-page.component';
import { RestorePageComponent } from './pages/restore/restore-page.component';
import { UserPageComponent } from './pages/users/user-page.component';
import { UsersPageComponent } from './pages/users/users-page.component';

export const ADMINISTRATION_ROUTES: Routes = [
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
                providers: [
                    EventConsumersService,
                    EventConsumersState,
                ],
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
                providers: [
                    UsersService,
                    UsersState,
                ],
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
                        canActivate: [userMustExistGuard],
                    },
                ],
            },
        ],
    },
];