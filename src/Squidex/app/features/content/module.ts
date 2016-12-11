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
    ContentPageComponent
} from './declarations';

const routes: Ng2Router.Routes = [
    {
        path: '',
        component: ContentPageComponent
    }
];

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        Ng2Router.RouterModule.forChild(routes)
    ],
    declarations: [
        ContentPageComponent
    ]
})
export class SqxFeatureContentModule { }