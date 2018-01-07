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
    ResolvePublishedSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    AssetsEditorComponent,
    ContentFieldComponent,
    ContentHistoryComponent,
    ContentPageComponent,
    ContentItemComponent,
    ContentsPageComponent,
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
                component: ContentsPageComponent,
                resolve: {
                    schema: ResolvePublishedSchemaGuard, appLanguages: ResolveAppLanguagesGuard
                },
                children: [
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canDeactivate: [CanDeactivateGuard],
                        children: [
                            {
                                path: 'assets',
                                loadChildren: './../assets/module#SqxFeatureAssetsModule'
                            },
                            {
                                path: 'references/:schemaName/:language',
                                component: ContentsPageComponent,
                                data: {
                                    isReadOnly: true
                                },
                                resolve: {
                                    schema: ResolvePublishedSchemaGuard
                                }
                            }
                        ]
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
                            },
                            {
                                path: 'references/:schemaName/:language',
                                component: ContentsPageComponent,
                                data: {
                                    isReadOnly: true
                                },
                                resolve: {
                                    schema: ResolvePublishedSchemaGuard
                                }
                            },
                            {
                                path: 'assets',
                                loadChildren: './../assets/module#SqxFeatureAssetsModule'
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
        ReferencesEditorComponent,
        SchemasPageComponent,
        SearchFormComponent
    ]
})
export class SqxFeatureContentModule { }