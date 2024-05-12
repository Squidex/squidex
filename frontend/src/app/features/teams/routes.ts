/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { HelpComponent, HistoryComponent } from '@app/shared';
import { TeamAuthState, TeamContributorsService, TeamContributorsState, TeamPlansService, TeamPlansState } from './internal';
import { AuthPageComponent } from './pages/auth/auth-page.component';
import { ContributorsPageComponent } from './pages/contributors/contributors-page.component';
import { DashboardPageComponent } from './pages/dashboard/dashboard-page.component';
import { MorePageComponent } from './pages/more/more-page.component';
import { PlansPageComponent } from './pages/plans/plans-page.component';
import { SettingsAreaComponent } from './shared/settings-area.component';
import { TeamAreaComponent } from './team-area.component';

export const TEAM_ROUTES: Routes = [
    {
        path: '',
        component: TeamAreaComponent,
        children: [
            {
                path: '',
                component: DashboardPageComponent,
            },
            {
                path: 'settings',
                component: SettingsAreaComponent,
                children: [
                    {
                        path: 'contributors',
                        component: ContributorsPageComponent,
                        providers: [
                            TeamContributorsService,
                            TeamContributorsState,
                        ],
                        children: [
                            {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'settings.contributors',
                                },
                            },
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/contributors-team',
                                },
                            },
                        ],
                    },
                    {
                        path: 'plans',
                        component: PlansPageComponent,
                        providers: [
                            TeamPlansService,
                            TeamPlansState,
                        ],
                        children: [
                            {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'settings.plan',
                                },
                            },
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/plans-team',
                                },
                            },
                        ],
                    },
                    {
                        path: 'auth',
                        component: AuthPageComponent,
                        providers: [
                            TeamAuthState,
                        ],
                        children: [
                            {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'settings.auth',
                                },
                            },
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/auth',
                                },
                            },
                        ],
                    },
                    {
                        path: 'more',
                        component: MorePageComponent,
                        children: [
                            {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'settings.general',
                                },
                            },
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/more-team',
                                },
                            },
                        ],
                    },
                ],
            },
        ],
    },
];
