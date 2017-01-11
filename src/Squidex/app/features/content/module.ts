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
    ResolvePublishedSchemaGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
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
                    schema: ResolvePublishedSchemaGuard
                },
                children: [
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        resolve: {
                            schema: ResolvePublishedSchemaGuard
                        },
                        data: {
                            disableHistory: true
                        }
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
                            schema: ResolvePublishedSchemaGuard
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
        ContentPageComponent,
        ContentsPageComponent,
        SchemasPageComponent
    ]
})
export class SqxFeatureContentModule { }