/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    FieldComponent,
    NumberUIComponent,
    NumberValidationComponent,
    SchemaFormComponent,
    SchemaPageComponent,
    SchemasPageComponent,
    StringUIComponent,
    StringValidationComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        children: [
            {
                path: ''
            },
            {
                path: ':schemaName',
                component: SchemaPageComponent
            }]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        FieldComponent,
        NumberUIComponent,
        NumberValidationComponent,
        SchemaFormComponent,
        SchemaPageComponent,
        SchemasPageComponent,
        StringUIComponent,
        StringValidationComponent
    ]
})
export class SqxFeatureSchemasModule { }