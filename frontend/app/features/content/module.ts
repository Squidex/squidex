/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CanDeactivateGuard, ContentMustExistGuard, LoadLanguagesGuard, LoadSchemasGuard, SchemaMustExistPublishedGuard, SchemaMustNotBeSingletonGuard, SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ScrollingModule as ScrollingModuleExperimental } from '@angular/cdk-experimental/scrolling';
import { ArrayEditorComponent, ArrayItemComponent, AssetsEditorComponent, CalendarPageComponent, CommentsPageComponent, ComponentComponent, ComponentSectionComponent, ContentComponent, ContentCreatorComponent, ContentEditorComponent, ContentEventComponent, ContentExtensionComponent, ContentFieldComponent, ContentHistoryPageComponent, ContentInspectionComponent, ContentPageComponent, ContentReferencesComponent, ContentSectionComponent, ContentsFiltersPageComponent, ContentsPageComponent, CustomViewEditorComponent, DueTimeSelectorComponent, FieldEditorComponent, FieldLanguagesComponent, IFrameEditorComponent, PreviewButtonComponent, ReferenceItemComponent, ReferencesEditorComponent, SchemasPageComponent, SidebarPageComponent, StockPhotoEditorComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: SchemasPageComponent,
        canActivate: [LoadLanguagesGuard, LoadSchemasGuard],
        children: [
            {
                path: '',
            },
            {
                path: '__calendar',
                component: CalendarPageComponent,
            },
            {
                path: ':schemaName',
                canActivate: [SchemaMustExistPublishedGuard],
                children: [
                    {
                        path: '',
                        component: ContentsPageComponent,
                        canActivate: [SchemaMustNotBeSingletonGuard, ContentMustExistGuard],
                        canDeactivate: [CanDeactivateGuard],
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
                        path: 'new',
                        component: ContentPageComponent,
                        canActivate: [SchemaMustNotBeSingletonGuard, ContentMustExistGuard],
                        canDeactivate: [CanDeactivateGuard],
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

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        ScrollingModule,
        ScrollingModuleExperimental,
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        ArrayEditorComponent,
        ArrayItemComponent,
        AssetsEditorComponent,
        CalendarPageComponent,
        CommentsPageComponent,
        ComponentComponent,
        ComponentSectionComponent,
        ContentComponent,
        ContentCreatorComponent,
        ContentEditorComponent,
        ContentEventComponent,
        ContentExtensionComponent,
        ContentFieldComponent,
        ContentHistoryPageComponent,
        ContentInspectionComponent,
        ContentPageComponent,
        ContentReferencesComponent,
        ContentSectionComponent,
        ContentsFiltersPageComponent,
        ContentsPageComponent,
        CustomViewEditorComponent,
        DueTimeSelectorComponent,
        FieldEditorComponent,
        FieldLanguagesComponent,
        IFrameEditorComponent,
        PreviewButtonComponent,
        ReferenceItemComponent,
        ReferencesEditorComponent,
        SchemasPageComponent,
        SidebarPageComponent,
        StockPhotoEditorComponent,
    ],
})
export class SqxFeatureContentModule {}
