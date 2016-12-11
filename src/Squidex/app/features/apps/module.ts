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
    AppsPageComponent
} from './declarations';

const routes: Ng2Router.Routes = [
    {
        path: '',
        component: AppsPageComponent
    }
];

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        Ng2Router.RouterModule.forChild(routes)
    ],
    declarations: [
        AppsPageComponent
    ]
})
export class SqxFeatureAppsModule { }