/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Http from '@angular/http';
import * as Ng2Forms from '@angular/forms';
import * as Ng2Common from '@angular/common';
import * as Ng2Router from '@angular/router';

import {
    AutocompleteComponent,
    CloakDirective,
    ColorPickerComponent,
    DayOfWeekPipe,
    DayPipe,
    DragModelDirective,
    DurationPipe,
    FocusOnChangeDirective,
    FocusOnInitDirective,
    ImageDropDirective,
    ModalViewDirective,
    MoneyPipe,
    MonthPipe,
    ScrollActiveDirective,
    ShortcutComponent,
    ShortDatePipe,
    ShortTimePipe,
    SliderComponent,
    SpinnerComponent,
    UserReportComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        Ng2Http.HttpModule,
        Ng2Forms.FormsModule,
        Ng2Forms.ReactiveFormsModule,
        Ng2Common.CommonModule,
        Ng2Router.RouterModule
    ],
    declarations: [
        AutocompleteComponent,
        CloakDirective,
        ColorPickerComponent,
        DayOfWeekPipe,
        DayPipe,
        DragModelDirective,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        ImageDropDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        UserReportComponent,
    ],
    exports: [
        AutocompleteComponent,
        CloakDirective,
        ColorPickerComponent,
        DayOfWeekPipe,
        DayPipe,
        DragModelDirective,
        DurationPipe,
        FocusOnChangeDirective,
        FocusOnInitDirective,
        ImageDropDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        UserReportComponent,
        Ng2Http.HttpModule,
        Ng2Forms.FormsModule,
        Ng2Forms.ReactiveFormsModule,
        Ng2Common.CommonModule,
        Ng2Router.RouterModule
    ]
})
export class SqxFrameworkModule { }