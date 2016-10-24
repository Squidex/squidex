/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { FrameworkModule } from './../../framework';

import {
    AppsComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        FrameworkModule
    ],
    declarations: [
        AppsComponent
    ]
})
export class MyAppModule { }