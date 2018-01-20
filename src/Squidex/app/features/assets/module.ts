/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    AssetsPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AssetsPageComponent
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        DndModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AssetsPageComponent
    ]
})
export class SqxFeatureAssetsModule { }