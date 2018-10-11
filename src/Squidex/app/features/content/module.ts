/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';
import { ColorPickerModule  } from 'ngx-color-picker';

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
                        canDeactivate: [CanDeactivateGuard]
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
        ColorPickerModule,
        DndModule,
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        ArrayEditorComponent,
        ArrayItemComponent,
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
        SchemasPageComponent
    ]
})
export class SqxFeatureContentModule { }