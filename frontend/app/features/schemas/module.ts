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
                        path: '',
                        redirectTo: 'fields'
                    },
                    {
                        path: 'fields',
                        children: [
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/schemas'
                                }
                            }
                        ]
                    },
                    {
                        path: 'scripts',
                        children: [
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/scripts'
                                }
                            }
                        ]
                    },
                    {
                        path: 'json',
                        children: [
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/schema-json'
                                }
                            }
                        ]
                    },
                    {
                        path: 'more',
                        children: [
                            {
                                path: 'help',
                                component: HelpComponent,
                                data: {
                                    helpPage: '05-integrated/preview'
                                }
                            }
                        ]
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
        StringUIComponent,
        StringValidationComponent,
        TagsUIComponent,
        TagsValidationComponent
    ]
})
export class SqxFeatureSchemasModule {}