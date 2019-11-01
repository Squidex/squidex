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
                redirectTo: ':schemaName/fields'
            },
            {
                path: ':schemaName/fields',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                data: {
                    tab: 'Fields'
                },
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
                path: ':schemaName/fields',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                data: {
                    tab: 'Fields'
                },
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
                path: ':schemaName/scripts',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                data: {
                    tab: 'Scripts'
                },
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
                path: ':schemaName/json',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                data: {
                    tab: 'Json'
                },
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/schemas-sync'
                        }
                    }
                ]
            },
            {
                path: ':schemaName/more',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                data: {
                    tab: 'More'
                },
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