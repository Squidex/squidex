/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    ClientComponent,
    ClientsPageComponent,
    ContributorsPageComponent,
    LanguagesPageComponent,
    SettingsAreaComponent
} from './declarations';

const routes: Ng2Router.Routes = [
    {
        path: '',
        component: SettingsAreaComponent,
        children: [
            {
                path: ''
            },
            {
                path: 'clients',
                component: ClientsPageComponent
            }, {
                path: 'contributors',
                component: ContributorsPageComponent
            }, {
                path: 'languages',
                component: LanguagesPageComponent
            }
        ]
    }
];

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        Ng2Router.RouterModule.forChild(routes)
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