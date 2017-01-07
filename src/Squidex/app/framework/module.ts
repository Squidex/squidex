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
    CopyDirective,
    DayOfWeekPipe,
    DayPipe,
    DurationPipe,
    FocusOnChangeDirective,
    FocusOnInitDirective,
    FromNowPipe,
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
        CopyDirective,
        DayOfWeekPipe,
        DayPipe,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        FromNowPipe,
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
        TagEditorComponent,
        TitleComponent,
        UserReportComponent
    ],
    exports: [
        AutocompleteComponent,
        CloakDirective,
        CopyDirective,
        DayOfWeekPipe,
        DayPipe,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        FromNowPipe,
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