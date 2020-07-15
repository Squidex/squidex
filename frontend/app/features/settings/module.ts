/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, HistoryComponent, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { BackupComponent, BackupsPageComponent, ClientAddFormComponent, ClientComponent, ClientConnectFormComponent, ClientsPageComponent, ContributorAddFormComponent, ContributorComponent, ContributorsPageComponent, ImportContributorsDialogComponent, LanguageAddFormComponent, LanguageComponent, LanguagesPageComponent, MorePageComponent, PatternComponent, PatternsPageComponent, PlanComponent, PlansPageComponent, RoleAddFormComponent, RoleComponent, RolesPageComponent, SettingsAreaComponent, WorkflowAddFormComponent, WorkflowComponent, WorkflowsPageComponent, WorkflowStepComponent, WorkflowTransitionComponent } from './declarations';
import { WorkflowDiagramComponent } from './pages/workflows/workflow-diagram.component';

const routes: Routes = [
    {
        path: '',
        component: SettingsAreaComponent,
        children: [
            {
                path: '',
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
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule
    ],
    declarations: [
        BackupComponent,
        BackupsPageComponent,
        ClientAddFormComponent,
        ClientComponent,
        ClientConnectFormComponent,
        ClientsPageComponent,
        ContributorAddFormComponent,
        ContributorComponent,
        ContributorsPageComponent,
        ImportContributorsDialogComponent,
        LanguageAddFormComponent,
        LanguageComponent,
        LanguagesPageComponent,
        MorePageComponent,
        PatternComponent,
        PatternsPageComponent,
        PlanComponent,
        PlansPageComponent,
        RoleAddFormComponent,
        RoleComponent,
        RolesPageComponent,
        SettingsAreaComponent,
        WorkflowAddFormComponent,
        WorkflowComponent,
        WorkflowDiagramComponent,
        WorkflowsPageComponent,
        WorkflowStepComponent,
        WorkflowTransitionComponent
    ]
})
export class SqxFeatureSettingsModule {}