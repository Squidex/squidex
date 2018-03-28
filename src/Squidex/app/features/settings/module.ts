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
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
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
                path: 'plans',
                component: PlansPageComponent
            },
            {
                path: 'more',
                component: MorePageComponent
            },
            {
                path: 'backups',
                component: BackupsPageComponent
            },
            {
                path: 'clients',
                component: ClientsPageComponent
            },
            {
                path: 'contributors',
                component: ContributorsPageComponent
            },
            {
                path: 'languages',
                component: LanguagesPageComponent
            },
            {
                path: 'patterns',
                component: PatternsPageComponent
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