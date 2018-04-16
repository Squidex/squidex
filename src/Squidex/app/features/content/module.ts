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
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    SchemaMustExistPublishedGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from '@app/shared';

import {
    AssetsEditorComponent,
    ContentFieldComponent,
    ContentHistoryComponent,
    ContentItemComponent,
    ContentPageComponent,
    ContentsPageComponent,
    ContentsSelectorComponent,
    ReferencesEditorComponent,
    SchemasPageComponent,
    SearchFormComponent
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
                canActivate: [SchemaMustExistPublishedGuard],
                resolve: {
                    appLanguages: ResolveAppLanguagesGuard
                },
                children: [
                    {
                        path: '',
                        component: ContentsPageComponent,
                        canDeactivate: [CanDeactivateGuard]
                    },
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canDeactivate: [CanDeactivateGuard]
                    },
                    {
                        path: ':contentId',
                        component: ContentPageComponent,
                        canDeactivate: [CanDeactivateGuard],
                        resolve: {
                            content: ResolveContentGuard
                        },
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
        DndModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AssetsEditorComponent,
        ContentFieldComponent,
        ContentHistoryComponent,
        ContentItemComponent,
        ContentPageComponent,
        ContentsPageComponent,
        ContentsSelectorComponent,
        ReferencesEditorComponent,
        SchemasPageComponent,
        SearchFormComponent
    ]
})
export class SqxFeatureContentModule { }