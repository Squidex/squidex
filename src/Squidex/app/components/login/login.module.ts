/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { FrameworkModule } from './../../framework';

import {
    LoginComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        FrameworkModule
    ],
    declarations: [
        LoginComponent
    ]
})
export class MyLoginModule { }