/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AssetsFiltersPageComponent, AssetsPageComponent, AssetTagsComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AssetsPageComponent,
        children: [
            {
                path: 'filters',
                component: AssetsFiltersPageComponent,
            },
        ],
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        AssetsFiltersPageComponent,
        AssetsPageComponent,
        AssetTagsComponent,
    ],
})
export class SqxFeatureAssetsModule {}
