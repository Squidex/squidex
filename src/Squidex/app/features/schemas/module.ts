/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import {
    HelpComponent,
    HistoryComponent,
    ResolveSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    FieldComponent,
    AssetsUIComponent,
    AssetsValidationComponent,
    BooleanUIComponent,
    BooleanValidationComponent,
    DateTimeUIComponent,
    DateTimeValidationComponent,
    GeolocationUIComponent,
    GeolocationValidationComponent,
    JsonUIComponent,
    JsonValidationComponent,
    NumberUIComponent,
    NumberValidationComponent,
    ReferencesUIComponent,
    ReferencesValidationComponent,
    SchemaEditFormComponent,
    SchemaFormComponent,
    SchemaPageComponent,
    SchemasPageComponent,
    SchemaScriptsFormComponent,
    StringUIComponent,
    StringValidationComponent,
    TagsUIComponent,
    TagsValidationComponent
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
                component: SchemaPageComponent,
                resolve: {
                    schema: ResolveSchemaGuard
                },
                children: [
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'schemas.{schemaName}'
                        }
                    },
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/schemas'
                        }
                    }
                ]
            }]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        DndModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        FieldComponent,
        AssetsUIComponent,
        AssetsValidationComponent,
        BooleanUIComponent,
        BooleanValidationComponent,
        DateTimeUIComponent,
        DateTimeValidationComponent,
        GeolocationUIComponent,
        GeolocationValidationComponent,
        JsonUIComponent,
        JsonValidationComponent,
        NumberUIComponent,
        NumberValidationComponent,
        ReferencesUIComponent,
        ReferencesValidationComponent,
        SchemaEditFormComponent,
        SchemaFormComponent,
        SchemaPageComponent,
        SchemaScriptsFormComponent,
        SchemasPageComponent,
        StringUIComponent,
        StringValidationComponent,
        TagsUIComponent,
        TagsValidationComponent
    ]
})
export class SqxFeatureSchemasModule { }