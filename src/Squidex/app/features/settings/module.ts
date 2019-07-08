/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HelpComponent,
    HistoryComponent,
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    BackupDurationPipe,
    BackupsPageComponent,
    ClientComponent,
    ClientsPageComponent,
    ContributorsPageComponent,
    LanguageComponent,
    LanguagesPageComponent,
    MorePageComponent,
    PatternComponent,
    PatternsPageComponent,
    PlansPageComponent,
    RoleComponent,
    RolesPageComponent,
    SettingsAreaComponent,
    WorkflowComponent,
    WorkflowsPageComponent,
    WorkflowStepComponent,
    WorkflowTransitionComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SettingsAreaComponent,
        children: [
            {
                path: ''
            },
            {
                path: 'more',
                component: MorePageComponent
            },
            {
                path: 'backups',
                component: BackupsPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/backups'
                        }
                    }
                ]
            },
            {
                path: 'clients',
                component: ClientsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.clients'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/clients'
                        }
                    }
                ]
            },
            {
                path: 'contributors',
                component: ContributorsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.contributors'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/contributors'
                        }
                    }
                ]
            },
            {
                path: 'languages',
                component: LanguagesPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.languages'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/languages'
                        }
                    }
                ]
            },
            {
                path: 'patterns',
                component: PatternsPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.patterns'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/patterns'
                        }
                    }
                ]
            },
            {
                path: 'plans',
                component: PlansPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.plan'
                        }
                    }
                ]
            },
            {
                path: 'roles',
                component: RolesPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.roles'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/roles'
                        }
                    }
                ]
            },
            {
                path: 'workflows',
                component: WorkflowsPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/workflows'
                        }
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
        BackupDurationPipe,
        BackupsPageComponent,
        ClientComponent,
        ClientsPageComponent,
        ContributorsPageComponent,
        LanguageComponent,
        LanguagesPageComponent,
        MorePageComponent,
        PatternComponent,
        PatternsPageComponent,
        PlansPageComponent,
        RoleComponent,
        RolesPageComponent,
        SettingsAreaComponent,
        WorkflowComponent,
        WorkflowsPageComponent,
        WorkflowTransitionComponent,
        WorkflowStepComponent
    ]
})
export class SqxFeatureSettingsModule {}