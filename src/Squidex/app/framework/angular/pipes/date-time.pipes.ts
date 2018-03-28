/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import { DateTime } from './../utils/date-time';
import { Duration } from './../utils/duration';

@Pipe({
    name: 'sqxShortDate',
    pure: true
})
export class ShortDatePipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('DD. MMM');
    }
}

@Pipe({
    name: 'sqxDate',
    pure: true
})
export class DatePipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('DD. MMM YYYY');
    }
}

@Pipe({
    name: 'sqxMonth',
    pure: true
})
export class MonthPipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('MMMM');
    }
}

@Pipe({
    name: 'sqxFromNow',
    pure: true
})
export class FromNowPipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toFromNow();
    }
}

@Pipe({
    name: 'sqxDayOfWeek',
    pure: true
})
export class DayOfWeekPipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('dd');
    }
}

@Pipe({
    name: 'sqxDay',
    pure: true
})
export class DayPipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('DD');
    }
}

@Pipe({
    name: 'sqxShortTime',
    pure: true
})
export class ShortTimePipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('HH:mm');
    }
}

@Pipe({
    name: 'sqxFullDateTime',
    pure: true
})
export class FullDateTimePipe implements PipeTransform {
    public transform(value: DateTime): any {
        return value.toStringFormat('LLLL');
    }
}

@Pipe({
    name: 'sqxDuration',
    pure: true
})
export class DurationPipe implements PipeTransform {
    public transform(value: Duration): any {
        return value.toString();
    }
}