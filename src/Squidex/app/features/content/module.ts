/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import {
    CanDeactivateGuard,
    HistoryComponent,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    AssetsEditorComponent,
    ContentFieldComponent,
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
                                    schemaOverride: ResolvePublishedSchemaGuard
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
                                component: HistoryComponent,
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
        ContentItemComponent,
        ContentPageComponent,
        ContentsPageComponent,
        ReferencesEditorComponent,
        SchemasPageComponent,
        SearchFormComponent
    ]
})
export class SqxFeatureContentModule { }