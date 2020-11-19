﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ColorPickerModule } from 'ngx-color-picker';
import { AnalyticsService, AutocompleteComponent, AvatarComponent, CachingInterceptor, CanDeactivateGuard, CheckboxGroupComponent, ClipboardService, CodeComponent, CodeEditorComponent, ColorPickerComponent, ConfirmClickDirective, ControlErrorsComponent, CopyDirective, DarkenPipe, DatePipe, DateTimeEditorComponent, DayOfWeekPipe, DayPipe, DialogRendererComponent, DialogService, DisplayNamePipe, DropdownComponent, DurationPipe, EditableTitleComponent, ExternalLinkDirective, FileDropDirective, FileSizePipe, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FromNowPipe, FullDateTimePipe, HighlightPipe, HoverBackgroundDirective, IFrameEditorComponent, ImageSourceDirective, IndeterminateValueDirective, ISODatePipe, JsonEditorComponent, KeysPipe, KNumberPipe, LanguageSelectorComponent, LightenPipe, ListViewComponent, LoadingInterceptor, LoadingService, LocalizedInputComponent, LocalStoreService, MarkdownInlinePipe, MarkdownPipe, MessageBus, ModalDialogComponent, ModalDirective, ModalPlacementDirective, MoneyPipe, MonthPipe, OnboardingService, OnboardingTooltipComponent, PagerComponent, PanelComponent, PanelContainerDirective, ParentLinkDirective, PopupLinkDirective, ProgressBarComponent, ResizedDirective, ResizeService, ResourceLoaderService, RootViewComponent, SafeHtmlPipe, SafeUrlPipe, ScrollActiveDirective, ShortcutComponent, ShortcutService, ShortDatePipe, ShortTimePipe, StarsComponent, StatusIconComponent, StopClickDirective, SyncScollingDirective, SyncWidthDirective, TabRouterlinkDirective, TagEditorComponent, TemplateWrapperDirective, TempService, TitleComponent, TitleService, ToggleComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from './declarations';

@NgModule({
    imports: [
        ColorPickerModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule
    ],
    declarations: [
        AutocompleteComponent,
        AvatarComponent,
        CheckboxGroupComponent,
        CodeComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        DarkenPipe,
        DatePipe,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DurationPipe,
        EditableTitleComponent,
        ExternalLinkDirective,
        FileDropDirective,
        FileSizePipe,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FromNowPipe,
        FullDateTimePipe,
        HighlightPipe,
        HoverBackgroundDirective,
        IFrameEditorComponent,
        ImageSourceDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JsonEditorComponent,
        KeysPipe,
        KNumberPipe,
        LanguageSelectorComponent,
        LightenPipe,
        ListViewComponent,
        LocalizedInputComponent,
        MarkdownInlinePipe,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        MoneyPipe,
        MonthPipe,
        OnboardingTooltipComponent,
        PagerComponent,
        PanelComponent,
        PanelContainerDirective,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
        ResizedDirective,
        RootViewComponent,
        SafeHtmlPipe,
        SafeUrlPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        StarsComponent,
        StatusIconComponent,
        StopClickDirective,
        SyncScollingDirective,
        SyncWidthDirective,
        TabRouterlinkDirective,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe
    ],
    exports: [
        AutocompleteComponent,
        AvatarComponent,
        CheckboxGroupComponent,
        CodeComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        CommonModule,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        DarkenPipe,
        DatePipe,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DurationPipe,
        EditableTitleComponent,
        ExternalLinkDirective,
        FileDropDirective,
        FileSizePipe,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        FromNowPipe,
        FullDateTimePipe,
        HighlightPipe,
        HoverBackgroundDirective,
        IFrameEditorComponent,
        ImageSourceDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JsonEditorComponent,
        KeysPipe,
        KNumberPipe,
        LanguageSelectorComponent,
        LightenPipe,
        ListViewComponent,
        LocalizedInputComponent,
        MarkdownInlinePipe,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        MoneyPipe,
        MonthPipe,
        OnboardingTooltipComponent,
        PagerComponent,
        PanelComponent,
        PanelContainerDirective,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
        ReactiveFormsModule,
        ResizedDirective,
        RootViewComponent,
        SafeHtmlPipe,
        SafeUrlPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        StarsComponent,
        StatusIconComponent,
        StopClickDirective,
        SyncScollingDirective,
        SyncWidthDirective,
        TabRouterlinkDirective,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe
    ]
})
export class SqxFrameworkModule {
    public static forRoot(): ModuleWithProviders<SqxFrameworkModule> {
        return {
            ngModule: SqxFrameworkModule,
            providers: [
                AnalyticsService,
                CanDeactivateGuard,
                ClipboardService,
                DialogService,
                LoadingService,
                LocalStoreService,
                MessageBus,
                OnboardingService,
                ResizeService,
                ResourceLoaderService,
                ShortcutService,
                TempService,
                TitleService,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: LoadingInterceptor,
                    multi: true
                },
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: CachingInterceptor,
                    multi: true
                }
            ]
        };
    }
 }