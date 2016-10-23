import * as Ng2 from '@angular/core';
import * as Ng2Http from '@angular/http';
import * as Ng2Forms from '@angular/forms';
import * as Ng2Common from '@angular/common';
import * as Ng2Router from '@angular/router';

import {
    CloakDirective,
    ColorPickerComponent,
    DayOfWeekPipe,
    DayPipe,
    DragModelDirective,
    DurationPipe,
    FocusOnChangeDirective,
    ImageDropDirective,
    MoneyPipe,
    MonthPipe,
    ShortcutComponent,
    ShortDatePipe,
    ShortTimePipe,
    SliderComponent,
    SpinnerComponent,
    UserReportComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        Ng2Forms.FormsModule,
        Ng2Common.CommonModule
    ],
    declarations: [
        CloakDirective,
        ColorPickerComponent,
        DayOfWeekPipe,
        DayPipe,
        DragModelDirective,
        DurationPipe,
        FocusOnChangeDirective,
        ImageDropDirective,
        MoneyPipe,
        MonthPipe,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        UserReportComponent
    ],
    exports: [
        CloakDirective,
        ColorPickerComponent,
        DayOfWeekPipe,
        DayPipe,
        DurationPipe,
        FocusOnChangeDirective,
        MoneyPipe,
        MonthPipe,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SpinnerComponent,
        UserReportComponent,
        Ng2Http.HttpModule,
        Ng2Forms.FormsModule,
        Ng2Common.CommonModule,
        Ng2Router.RouterModule
    ]
})
export class FrameworkModule { }