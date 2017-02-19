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
    EventConsumersPageComponent,
    UsersPageComponent
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
                    component: EventConsumersPageComponent
                }, {
                    path: 'users',
                    component: UsersPageComponent
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
        EventConsumersPageComponent,
        UsersPageComponent
    ]
})
export class SqxFeatureAdministrationModule { }