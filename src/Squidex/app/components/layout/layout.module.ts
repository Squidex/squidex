/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule } from './../../framework';

import {
    AppFormComponent,
    AppsMenuComponent,
    SearchFormComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AppsMenuComponent,
        SearchFormComponent,
    ],
    exports: [
        AppFormComponent,
        AppsMenuComponent,
        SearchFormComponent,
    ]
})
export class SqxLayoutModule { }