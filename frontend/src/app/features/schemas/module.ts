/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HelpComponent, HistoryComponent, LoadSchemasGuard, SchemaMustExistGuard, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { ArrayValidationComponent, AssetsUIComponent, AssetsValidationComponent, BooleanUIComponent, BooleanValidationComponent, ComponentsUIComponent, ComponentsValidationComponent, ComponentUIComponent, ComponentValidationComponent, DateTimeUIComponent, DateTimeValidationComponent, FieldComponent, FieldFormCommonComponent, FieldFormComponent, FieldFormUIComponent, FieldFormValidationComponent, FieldGroupComponent, FieldListComponent, FieldWizardComponent, GeolocationUIComponent, GeolocationValidationComponent, JsonMoreComponent, JsonUIComponent, JsonValidationComponent, NumberUIComponent, NumberValidationComponent, ReferencesUIComponent, ReferencesValidationComponent, SchemaEditFormComponent, SchemaExportFormComponent, SchemaFieldRulesFormComponent, SchemaFieldsComponent, SchemaFormComponent, SchemaPageComponent, SchemaPreviewUrlsFormComponent, SchemaScriptNamePipe, SchemaScriptsFormComponent, SchemasPageComponent, SchemaUIFormComponent, SortableFieldListComponent, StringUIComponent, StringValidationComponent, TagsUIComponent, TagsValidationComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        canActivate: [LoadSchemasGuard],
        children: [
            {
                path: ':schemaName',
                component: SchemaPageComponent,
                canActivate: [SchemaMustExistGuard],
                children: [
                    {
                        path: 'help',
                        component: HelpComponent,
                        data: {
                            helpPage: '05-integrated/schemas',
                        },
                    },
                    {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'schemas.{schemaId}',
                        },
                    },
                ],
            },
        ],
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    providers: [
        SchemaMustExistGuard,
    ],
    declarations: [
        ArrayValidationComponent,
        AssetsUIComponent,
        AssetsValidationComponent,
        BooleanUIComponent,
        BooleanValidationComponent,
        ComponentUIComponent,
        ComponentValidationComponent,
        ComponentsUIComponent,
        ComponentsValidationComponent,
        DateTimeUIComponent,
        DateTimeValidationComponent,
        FieldComponent,
        FieldGroupComponent,
        FieldFormCommonComponent,
        FieldFormComponent,
        FieldFormUIComponent,
        FieldFormValidationComponent,
        FieldListComponent,
        FieldWizardComponent,
        GeolocationUIComponent,
        GeolocationValidationComponent,
        JsonMoreComponent,
        JsonUIComponent,
        JsonValidationComponent,
        NumberUIComponent,
        NumberValidationComponent,
        ReferencesUIComponent,
        ReferencesValidationComponent,
        SchemaEditFormComponent,
        SchemaExportFormComponent,
        SchemaFieldRulesFormComponent,
        SchemaFieldsComponent,
        SchemaFormComponent,
        SchemaPageComponent,
        SchemaPreviewUrlsFormComponent,
        SchemaScriptsFormComponent,
        SchemaScriptNamePipe,
        SchemasPageComponent,
        SchemaUIFormComponent,
        SortableFieldListComponent,
        StringUIComponent,
        StringValidationComponent,
        TagsUIComponent,
        TagsValidationComponent,
    ],
})
export class SqxFeatureSchemasModule {}
