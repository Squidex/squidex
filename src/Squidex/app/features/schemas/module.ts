/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import {
    HelpComponent,
    HistoryComponent,
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
    FieldFormCommonComponent,
    FieldFormUIComponent,
    FieldFormValidationComponent,
    FieldWizardComponent,
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
    SchemaMustExistGuard,
    SchemaPageComponent,
    SchemasPageComponent,
    SchemaScriptsFormComponent,
    SchemasState,
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
                canActivate: [SchemaMustExistGuard],
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
    providers: [
        SchemaMustExistGuard,
        SchemasState
    ],
    declarations: [
        FieldComponent,
        AssetsUIComponent,
        AssetsValidationComponent,
        BooleanUIComponent,
        BooleanValidationComponent,
        DateTimeUIComponent,
        DateTimeValidationComponent,
        FieldFormCommonComponent,
        FieldFormUIComponent,
        FieldFormValidationComponent,
        FieldWizardComponent,
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