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
    CanDeactivateGuard,
    ContentMustExistGuard,
    LoadLanguagesGuard,
    SchemaMustExistPublishedGuard,
    SchemaMustNotBeSingletonGuard,
    SqxFrameworkModule,
    SqxSharedModule,
    UnsetContentGuard
} from '@app/shared';

import {
    ArrayEditorComponent,
    ArrayItemComponent,
    AssetsEditorComponent,
    CommentsPageComponent,
    ContentFieldComponent,
    ContentHistoryPageComponent,
    ContentItemComponent,
    ContentPageComponent,
    ContentsFiltersPageComponent,
    ContentsPageComponent,
    ContentsSelectorComponent,
    ContentStatusComponent,
    ContentValueComponent,
    ContentValueEditorComponent,
    DueTimeSelectorComponent,
    FieldEditorComponent,
    FieldLanguagesComponent,
    PreviewButtonComponent,
    ReferencesDropdownComponent,
    ReferencesEditorComponent,
    SchemasPageComponent
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
                        canActivate: [SchemaMustNotBeSingletonGuard],
                        canDeactivate: [CanDeactivateGuard],
                        children: [
                            {
                                path: 'filters',
                                component: ContentsFiltersPageComponent
                            }
                        ]
                    },
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canActivate: [SchemaMustNotBeSingletonGuard, UnsetContentGuard],
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
                                component: ContentHistoryPageComponent,
                                data: {
                                    channel: 'contents.{contentId}'
                                }
                            },
                            {
                               path: 'comments',
                               component: CommentsPageComponent
                           }
                        ]
                    }
                ]
            }]
    }
];

@NgModule({
    imports: [
        DndModule,
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        ArrayEditorComponent,
        ArrayItemComponent,
        AssetsEditorComponent,
        CommentsPageComponent,
        ContentFieldComponent,
        ContentHistoryPageComponent,
        ContentItemComponent,
        ContentPageComponent,
        ContentsFiltersPageComponent,
        ContentsPageComponent,
        ContentsSelectorComponent,
        ContentStatusComponent,
        ContentValueComponent,
        ContentValueEditorComponent,
        DueTimeSelectorComponent,
        FieldEditorComponent,
        FieldLanguagesComponent,
        PreviewButtonComponent,
        ReferencesDropdownComponent,
        ReferencesEditorComponent,
        SchemasPageComponent
    ]
})
export class SqxFeatureContentModule {}