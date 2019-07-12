/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    ApiAreaComponent,
    GraphQLPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: ApiAreaComponent,
        children: [
            {
                path: ''
            },
            {
                path: 'graphql',
                component: GraphQLPageComponent
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
        ApiAreaComponent,
        GraphQLPageComponent
    ]
})
export class SqxFeatureApiModule {}