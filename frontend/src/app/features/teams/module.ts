/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GridsterModule } from 'angular-gridster2';
import { HelpComponent, HistoryComponent, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { ContributorAddFormComponent, ContributorComponent, ContributorsPageComponent, DashboardPageComponent, ImportContributorsDialogComponent, LeftMenuComponent, MorePageComponent, PlanComponent, PlansPageComponent, SettingsAreaComponent, SettingsMenuComponent, TeamAreaComponent } from './declarations';
import { TeamContributorsService, TeamContributorsState, TeamPlansService, TeamPlansState } from './internal';

const routes: Routes = [
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
                                    helpPage: '05-integrated/team-contributors',
                                },
                            },
                        ],
                    },
                    {
                        path: 'plans',
                        component: PlansPageComponent,
                        children: [
                            {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'settings.plan',
                                },
                            },
                        ],
                    },
                    {
                        path: 'more',
                        component: MorePageComponent,
                    },
                ],
            },
        ],
    },
];

@NgModule({
    imports: [
        GridsterModule,
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        DashboardPageComponent,
        ContributorAddFormComponent,
        ContributorComponent,
        ContributorsPageComponent,
        ImportContributorsDialogComponent,
        LeftMenuComponent,
        MorePageComponent,
        PlanComponent,
        PlansPageComponent,
        SettingsAreaComponent,
        SettingsMenuComponent,
        TeamAreaComponent,
    ],
    providers: [
        TeamContributorsService,
        TeamContributorsState,
        TeamPlansService,
        TeamPlansState,
    ],
})
export class SqxFeatureTeamsModule {}
