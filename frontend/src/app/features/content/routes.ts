/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Routes } from '@angular/router';
import { canDeactivateGuard, contentMustExistGuard, loadLanguagesGuard, loadSchemasGuard, schemaMustExistPublishedGuard, schemaMustNotBeSingletonGuard } from '@app/shared';
import { CalendarPageComponent } from './pages/calendar/calendar-page.component';
import { CommentsPageComponent } from './pages/comments/comments-page.component';
import { ContentHistoryPageComponent } from './pages/content/content-history-page.component';
import { ContentPageComponent } from './pages/content/content-page.component';
import { ContentsFiltersPageComponent } from './pages/contents/contents-filters-page.component';
import { ContentsPageComponent } from './pages/contents/contents-page.component';
import { ContentsPluginComponent } from './pages/contents-plugin/contents-plugin.component';
import { ReferencesPageComponent } from './pages/references/references-page.component';
import { SchemasPageComponent } from './pages/schemas/schemas-page.component';
import { SidebarPageComponent } from './pages/sidebar/sidebar-page.component';

export const CONTENT_ROUTES: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        canActivate: [loadLanguagesGuard, loadSchemasGuard],
        children: [
            {
                path: '__calendar',
                component: CalendarPageComponent,
            },
            {
                path: '__references/:referenceId',
                component: ReferencesPageComponent,
            },
            {
                path: ':schemaName',
                canActivate: [schemaMustExistPublishedGuard],
                children: [
                    {
                        path: '',
                        component: ContentsPageComponent,
                        canActivate: [schemaMustNotBeSingletonGuard(false), contentMustExistGuard],
                        canDeactivate: [canDeactivateGuard],
                        children: [
                            {
                                path: 'filters',
                                component: ContentsFiltersPageComponent,
                            },
                            {
                                path: 'sidebar',
                                component: SidebarPageComponent,
                            },
                        ],
                    },
                    {
                        path: 'extension',
                        canActivate: [schemaMustNotBeSingletonGuard(true), contentMustExistGuard],
                        component: ContentsPluginComponent,
                    },
                    {
                        path: 'new',
                        component: ContentPageComponent,
                        canActivate: [schemaMustNotBeSingletonGuard, contentMustExistGuard],
                        canDeactivate: [canDeactivateGuard],
                        data: {
                            reuseId: 'contentPage',
                        },
                    },
                    {
                        path: ':contentId',
                        component: ContentPageComponent,
                        canActivate: [contentMustExistGuard],
                        canDeactivate: [canDeactivateGuard],
                        data: {
                            reuseId: 'contentPage',
                        },
                        children: [
                            {
                                path: 'history',
                                component: ContentHistoryPageComponent,
                                data: {
                                    channel: 'contents.{contentId}',
                                },
                            },
                            {
                                path: 'comments',
                                component: CommentsPageComponent,
                            },
                            {
                                path: 'sidebar',
                                component: SidebarPageComponent,
                            },
                        ],
                    },
                ],
            }],
    },
];