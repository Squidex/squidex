/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    AdministrationAreaComponent,
    EventConsumersPage
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AdministrationAreaComponent,
        children: [
            {
                path: '',
                children: [{
                    path: 'event-consumers',
                    component: EventConsumersPage
                }]
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
        AdministrationAreaComponent,
        EventConsumersPage
    ]
})
export class SqxFeatureAdministrationModule { }