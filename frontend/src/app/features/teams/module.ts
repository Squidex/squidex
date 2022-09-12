/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { LeftMenuComponent, SettingsAreaComponent, SettingsMenuComponent, TeamAreaComponent } from './declarations';
import { TeamContributorsService, TeamPlansService } from './internal';

const routes: Routes = [
    {
        path: '',
        component: TeamAreaComponent,
        children: [
            {
                path: '',
            },
            {
                path: 'settings',
                component: SettingsAreaComponent,
                children: [
                    {
                        path: 'contributors',
                    },
                    {
                        path: 'plans',
                    },
                    {
                        path: 'settings',
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
        LeftMenuComponent,
        SettingsMenuComponent,
        SettingsAreaComponent,
        TeamAreaComponent,
    ],
    providers: [
        TeamContributorsService,
        TeamPlansService,
    ],
})
export class SqxFeatureTeamsModule {}
