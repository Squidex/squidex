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
    ProfileMenuComponent,
    SearchFormComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AppsMenuComponent,
        ProfileMenuComponent,
        SearchFormComponent
    ],
    exports: [
        AppFormComponent,
        AppsMenuComponent,
        ProfileMenuComponent,
        SearchFormComponent
    ]
})
export class SqxLayoutModule { }