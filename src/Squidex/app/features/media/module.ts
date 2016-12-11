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
    MediaPageComponent
} from './declarations';

const routes: Ng2Router.Routes = [
    {
        path: '',
        component: MediaPageComponent
    }
];

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        Ng2Router.RouterModule.forChild(routes)
    ],
    declarations: [
        MediaPageComponent
    ]
})
export class SqxFeatureMediaModule { }