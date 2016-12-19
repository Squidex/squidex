/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import {
    AutocompleteComponent,
    CloakDirective,
    ColorPickerComponent,
    CopyDirective,
    DayOfWeekPipe,
    DayPipe,
    DragModelDirective,
    DurationPipe,
    FocusOnChangeDirective,
    FocusOnInitDirective,
    FromNowPipe,
    ImageDropDirective,
    ModalViewDirective,
    MoneyPipe,
    MonthPipe,
    PanelContainerDirective,
    PanelDirective,
    ScrollActiveDirective,
    ShortcutComponent,
    ShortDatePipe,
    ShortTimePipe,
    SliderComponent,
    SpinnerComponent,
    TagEditorComponent,
    TitleComponent,
    UserReportComponent
} from './declarations';

@NgModule({
    imports: [
        HttpModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule
    ],
    declarations: [
        AutocompleteComponent,
        CloakDirective,
        ColorPickerComponent,
        CopyDirective,
        DayOfWeekPipe,
        DayPipe,
        DragModelDirective,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        FromNowPipe,
        ImageDropDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        PanelContainerDirective,
        PanelDirective,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        TagEditorComponent,
        TitleComponent,
        UserReportComponent
    ],
    exports: [
        AutocompleteComponent,
        CloakDirective,
        ColorPickerComponent,
        CopyDirective,
        DayOfWeekPipe,
        DayPipe,
        DragModelDirective,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        FromNowPipe,
        ImageDropDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        PanelContainerDirective,
        PanelDirective,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        TagEditorComponent,
        TitleComponent,
        UserReportComponent,
        HttpModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule
    ]
})
export class SqxFrameworkModule { }