/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import {
    HelpComponent,
    HistoryComponent,
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    BackupDownloadUrlPipe,
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
    SettingsAreaComponent
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
                path: 'plans',
                component: PlansPageComponent,
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'settings.plans'
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
            }
        ]
    }
];

@NgModule({
    imports: [
        DndModule,
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        BackupDownloadUrlPipe,
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
        SettingsAreaComponent
    ]
})
export class SqxFeatureSettingsModule { }