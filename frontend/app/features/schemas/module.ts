/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HelpComponent,
    SchemaMustExistGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    ArrayValidationComponent,
    AssetsUIComponent,
    AssetsValidationComponent,
    BooleanUIComponent,
    BooleanValidationComponent,
    DateTimeUIComponent,
    DateTimeValidationComponent,
    FieldComponent,
    FieldFormCommonComponent,
    FieldFormComponent,
    FieldFormUIComponent,
    FieldFormValidationComponent,
    FieldListComponent,
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
    SchemaExportFormComponent,
    SchemaFieldsComponent,
    SchemaFormComponent,
    SchemaPageComponent,
    SchemaPreviewUrlsFormComponent,
    SchemaScriptsFormComponent,
    SchemasPageComponent,
    SchemaUIFormComponent,
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
                path: ':schemaName',
                canActivate: [SchemaMustExistGuard],
                component: SchemaPageComponent,
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/schemas'
                        }
                    }
                ]
            }
        ]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    providers: [
        SchemaMustExistGuard
    ],
    declarations: [
        ArrayValidationComponent,
        AssetsUIComponent,
        AssetsValidationComponent,
        BooleanUIComponent,
        BooleanValidationComponent,
        DateTimeUIComponent,
        DateTimeValidationComponent,
        FieldComponent,
        FieldFormCommonComponent,
        FieldFormComponent,
        FieldFormUIComponent,
        FieldFormValidationComponent,
        FieldListComponent,
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
        SchemaExportFormComponent,
        SchemaFieldsComponent,
        SchemaFormComponent,
        SchemaPageComponent,
        SchemaPreviewUrlsFormComponent,
        SchemaScriptsFormComponent,
        SchemasPageComponent,
        SchemaUIFormComponent,
        StringUIComponent,
        StringValidationComponent,
        TagsUIComponent,
        TagsValidationComponent
    ]
})
export class SqxFeatureSchemasModule {}