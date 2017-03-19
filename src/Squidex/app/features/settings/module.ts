/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HelpComponent,
    HistoryComponent,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    ClientComponent,
    ClientsPageComponent,
    ContributorsPageComponent,
    LanguagesPageComponent,
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
            }, {
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
            }, {
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
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        ClientComponent,
        ClientsPageComponent,
        ContributorsPageComponent,
        LanguagesPageComponent,
        SettingsAreaComponent
    ]
})
export class SqxFeatureSettingsModule { }