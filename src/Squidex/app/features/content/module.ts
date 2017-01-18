/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HistoryComponent,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    ContentFieldComponent,
    ContentPageComponent,
    ContentsPageComponent,
    SchemasPageComponent
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
                        component: ContentPageComponent
                    }, {
                        path: 'history',
                        component: HistoryComponent,
                        data: {
                            channel: 'contents.{schemaName}'
                        }
                    }, {
                        path: ':contentId',
                        component: ContentPageComponent,
                        resolve: {
                            content: ResolveContentGuard
                        },
                        children: [
                             {
                                path: 'history',
                                component: HistoryComponent,
                                data: {
                                    channel: 'contents.{schemaName}.{contentId}'
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
        ContentFieldComponent,
        ContentPageComponent,
        ContentsPageComponent,
        SchemasPageComponent
    ]
})
export class SqxFeatureContentModule { }