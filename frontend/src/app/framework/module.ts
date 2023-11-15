/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ErrorHandler, Injector, ModuleWithProviders, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ColorPickerModule } from 'ngx-color-picker';
import { TourService as BaseTourService } from 'ngx-ui-tour-core';
import { AnalyticsService, AutocompleteComponent, AvatarComponent, CachingInterceptor, CanDeactivateGuard, CheckboxGroupComponent, ClipboardService, CodeComponent, CodeEditorComponent, ColorPickerComponent, CompensateScrollbarDirective, ConfirmClickDirective, ControlErrorsComponent, ControlErrorsMessagesComponent, CopyDirective, CopyGlobalDirective, DarkenPipe, DatePipe, DateTimeEditorComponent, DayOfWeekPipe, DayPipe, DialogRendererComponent, DialogService, DisplayNamePipe, DropdownComponent, DropdownMenuComponent, DurationPipe, EditableTitleComponent, ExternalLinkDirective, FileDropDirective, FileSizePipe, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, FromNowPipe, FullDateTimePipe, GlobalErrorHandler, HighlightPipe, HoverBackgroundDirective, IfOnceDirective, ImageSourceDirective, ImageUrlDirective, IndeterminateValueDirective, ISODatePipe, JoinPipe, KeysPipe, KNumberPipe, LanguageSelectorComponent, LayoutComponent, LayoutContainerDirective, LightenPipe, ListViewComponent, LoaderComponent, LoadingInterceptor, LoadingService, LocalizedInputComponent, LocalStoreService, LongHoverDirective, MarkdownDirective, MarkdownInlinePipe, MarkdownPipe, MessageBus, ModalDialogComponent, ModalDirective, ModalPlacementDirective, MonthPipe, PagerComponent, ParentLinkDirective, ProgressBarComponent, RadioGroupComponent, ResizedDirective, ResizeService, ResourceLoaderService, RootViewComponent, SafeHtmlPipe, SafeResourceUrlPipe, SafeUrlPipe, ScrollActiveDirective, ShortcutComponent, ShortcutDirective, ShortcutService, ShortDatePipe, ShortTimePipe, SidebarMenuDirective, StarsComponent, StatusIconComponent, StopClickDirective, StopDragDirective, StringColorPipe, SyncScollingDirective, SyncWidthDirective, TabRouterlinkDirective, TagEditorComponent, TemplateWrapperDirective, TempService, TitleComponent, TitleService, ToggleComponent, ToolbarComponent, TooltipDirective, TourService, TourStepDirective, TourTemplateComponent, TransformInputDirective, TranslatePipe, VideoPlayerComponent } from './declarations';

@NgModule({
    imports: [
        ColorPickerModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        AutocompleteComponent,
        AvatarComponent,
        CheckboxGroupComponent,
        CodeComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        CompensateScrollbarDirective,
        ConfirmClickDirective,
        ControlErrorsComponent,
        ControlErrorsMessagesComponent,
        CopyDirective,
        CopyGlobalDirective,
        DarkenPipe,
        DatePipe,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DropdownMenuComponent,
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
        IfOnceDirective,
        ImageSourceDirective,
        ImageUrlDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JoinPipe,
        KeysPipe,
        KNumberPipe,
        LanguageSelectorComponent,
        LayoutComponent,
        LayoutContainerDirective,
        LightenPipe,
        ListViewComponent,
        LoaderComponent,
        LocalizedInputComponent,
        LongHoverDirective,
        MarkdownDirective,
        MarkdownInlinePipe,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        MonthPipe,
        PagerComponent,
        ParentLinkDirective,
        ProgressBarComponent,
        RadioGroupComponent,
        ResizedDirective,
        RootViewComponent,
        SafeHtmlPipe,
        SafeResourceUrlPipe,
        SafeUrlPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortcutDirective,
        ShortDatePipe,
        ShortTimePipe,
        SidebarMenuDirective,
        StarsComponent,
        StatusIconComponent,
        StopClickDirective,
        StopDragDirective,
        StringColorPipe,
        SyncScollingDirective,
        SyncWidthDirective,
        TabRouterlinkDirective,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        ToolbarComponent,
        TooltipDirective,
        TourStepDirective,
        TourTemplateComponent,
        TransformInputDirective,
        TranslatePipe,
        VideoPlayerComponent,
    ],
    exports: [
        AutocompleteComponent,
        AvatarComponent,
        CheckboxGroupComponent,
        CodeComponent,
        CodeEditorComponent,
        ColorPickerComponent,
        CommonModule,
        CompensateScrollbarDirective,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        CopyGlobalDirective,
        DarkenPipe,
        DatePipe,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DropdownMenuComponent,
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
        IfOnceDirective,
        ImageSourceDirective,
        ImageUrlDirective,
        IndeterminateValueDirective,
        ISODatePipe,
        JoinPipe,
        KeysPipe,
        KNumberPipe,
        LanguageSelectorComponent,
        LayoutComponent,
        LayoutContainerDirective,
        LightenPipe,
        ListViewComponent,
        LoaderComponent,
        LocalizedInputComponent,
        LongHoverDirective,
        MarkdownDirective,
        MarkdownInlinePipe,
        MarkdownPipe,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        MonthPipe,
        PagerComponent,
        ParentLinkDirective,
        ProgressBarComponent,
        RadioGroupComponent,
        ReactiveFormsModule,
        ResizedDirective,
        RootViewComponent,
        SafeHtmlPipe,
        SafeResourceUrlPipe,
        SafeUrlPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortcutDirective,
        ShortDatePipe,
        ShortTimePipe,
        SidebarMenuDirective,
        StarsComponent,
        StatusIconComponent,
        StopClickDirective,
        StopDragDirective,
        StringColorPipe,
        SyncScollingDirective,
        SyncWidthDirective,
        TabRouterlinkDirective,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        ToolbarComponent,
        TooltipDirective,
        TourStepDirective,
        TourTemplateComponent,
        TransformInputDirective,
        TranslatePipe,
        VideoPlayerComponent,
    ],
})
export class SqxFrameworkModule {
    constructor(injector: Injector) {
        injector.get(AnalyticsService);
    }

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
                ResizeService,
                ResourceLoaderService,
                ShortcutService,
                TempService,
                TourService,
                TitleService,
                {
                    provide: BaseTourService,
                    useClass: TourService,
                },
                {
                    provide: ErrorHandler,
                    useClass: GlobalErrorHandler,
                    multi: false,
                },
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: LoadingInterceptor,
                    multi: true,
                },
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: CachingInterceptor,
                    multi: true,
                },
            ],
        };
    }
}
