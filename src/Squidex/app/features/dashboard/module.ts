/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { SqxFrameworkModule } from 'shared';

import {
    DashboardPageComponent
} from './declarations';

const routes: Ng2Router.Routes = [
    {
        path: '',
        component: DashboardPageComponent
    }
];

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        Ng2Router.RouterModule.forChild(routes)
    ],
    declarations: [
        DashboardPageComponent
    ]
})
export class SqxFeatureDashboardModule { }