/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule } from 'shared';

import {
    AppFormComponent,
    AppsMenuComponent,
    AppsMenuListComponent,
    SearchFormComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AppsMenuComponent,
        AppsMenuListComponent,
        SearchFormComponent,
    ],
    exports: [
        AppFormComponent,
        AppsMenuComponent,
        AppsMenuListComponent,
        SearchFormComponent,
    ]
})
export class SqxLayoutModule { }