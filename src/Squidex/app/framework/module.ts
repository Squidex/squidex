﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ColorPickerModule  } from 'ngx-color-picker';

import {
    AnalyticsService,
    AutocompleteComponent,
    AvatarComponent,
    CachingInterceptor,
    CanDeactivateGuard,
    CheckboxGroupComponent,
    ClipboardService,
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
    DialogService,
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
    IgnoreScrollbarDirective,
    ImageSourceDirective,
    IndeterminateValueDirective,
    ISODatePipe,
    JsonEditorComponent,
    KeysPipe,
    KNumberPipe,
    LightenPipe,
    LoadingInterceptor,
    LoadingService,
    LocalStoreService,
    MarkdownPipe,
    MessageBus,
    ModalDialogComponent,
    ModalDirective,
    ModalPlacementDirective,
    MoneyPipe,
    MonthPipe,
    OnboardingService,
    OnboardingTooltipComponent,
    PagerComponent,
    PanelComponent,
    PanelContainerDirective,
    ParentLinkDirective,
    PopupLinkDirective,
    ProgressBarComponent,
    ResourceLoaderService,
    RootViewComponent,
    SafeHtmlPipe,
    SafeUrlPipe,
    ScrollActiveDirective,
    ShortcutComponent,
    ShortcutService,
    ShortDatePipe,
    ShortTimePipe,
    StarsComponent,
    StatusIconComponent,
    StopClickDirective,
    SyncScollingDirective,
    TagEditorComponent,
    TemplateWrapperDirective,
    TitleComponent,
    TitleService,
    ToggleComponent,
    TooltipDirective,
    TransformInputDirective
} from './declarations';

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
        ColorPickerComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CodeComponent,
        CodeEditorComponent,
        CopyDirective,
        DarkenPipe,
        DateTimeEditorComponent,
        DatePipe,
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
        IgnoreScrollbarDirective,
        ImageSourceDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JsonEditorComponent,
        KeysPipe,
        KNumberPipe,
        LightenPipe,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        MoneyPipe,
        MonthPipe,
        OnboardingTooltipComponent,
        PagerComponent,
        PanelContainerDirective,
        PanelComponent,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
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
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TransformInputDirective
    ],
    exports: [
        AutocompleteComponent,
        AvatarComponent,
        CheckboxGroupComponent,
        CodeEditorComponent,
        CommonModule,
        CodeComponent,
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
        FormsModule,
        FromNowPipe,
        FullDateTimePipe,
        HighlightPipe,
        HoverBackgroundDirective,
        IFrameEditorComponent,
        IgnoreScrollbarDirective,
        ImageSourceDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JsonEditorComponent,
        KeysPipe,
        KNumberPipe,
        LightenPipe,
        MarkdownPipe,
        ModalDirective,
        ModalDialogComponent,
        ModalPlacementDirective,
        MoneyPipe,
        MonthPipe,
        OnboardingTooltipComponent,
        PagerComponent,
        PanelContainerDirective,
        PanelComponent,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
        ReactiveFormsModule,
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
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        TooltipDirective,
        TransformInputDirective
    ]
})
export class SqxFrameworkModule {
    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: SqxFrameworkModule,
            providers: [
                AnalyticsService,
                CanDeactivateGuard,
                ClipboardService,
                DialogService,
                LocalStoreService,
                LoadingService,
                MessageBus,
                OnboardingService,
                ResourceLoaderService,
                ShortcutService,
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