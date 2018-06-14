/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    CanDeactivateGuard,
    ContentMustExistGuard,
    LoadLanguagesGuard,
    SchemaMustExistPublishedGuard,
    SqxFrameworkModule,
    SqxSharedModule,
    UnsetContentGuard
} from '@app/shared';

import {
    ArrayEditorComponent,
    AssetsEditorComponent,
    ContentFieldComponent,
    ContentHistoryComponent,
    ContentItemComponent,
    ContentPageComponent,
    ContentsPageComponent,
    ContentsSelectorComponent,
    ContentStatusComponent,
    DueTimeSelectorComponent,
    FieldEditorComponent,
    ReferencesEditorComponent,
    SchemasPageComponent,
    SearchFormComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        canActivate: [LoadLanguagesGuard],
        children: [
            {
                path: ''
            },
            {
                path: ':schemaName',
                canActivate: [SchemaMustExistPublishedGuard],
                children: [
                    {
                        path: '',
                        component: ContentsPageComponent,
                        canDeactivate: [CanDeactivateGuard]
                    },
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canActivate: [UnsetContentGuard],
                        canDeactivate: [CanDeactivateGuard]
                    },
                    {
                        path: ':contentId',
                        component: ContentPageComponent,
                        canActivate: [ContentMustExistGuard],
                        canDeactivate: [CanDeactivateGuard],
                        children: [
                             {
                                path: 'history',
                                component: ContentHistoryComponent,
                                data: {
                                    channel: 'contents.{contentId}'
                                }
                            }
                        ]
                    }
                ]
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
        ArrayEditorComponent,
        AssetsEditorComponent,
        ContentFieldComponent,
        ContentHistoryComponent,
        ContentItemComponent,
        ContentPageComponent,
        ContentStatusComponent,
        ContentsPageComponent,
        ContentsSelectorComponent,
        DueTimeSelectorComponent,
        FieldEditorComponent,
        ReferencesEditorComponent,
        SchemasPageComponent,
        SearchFormComponent
    ]
})
export class SqxFeatureContentModule { }