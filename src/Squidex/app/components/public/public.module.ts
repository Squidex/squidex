/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule } from './../../framework';

import {
    NotFoundPageComponent,
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        NotFoundPageComponent
    ]
})
export class SqxPublicModule { }