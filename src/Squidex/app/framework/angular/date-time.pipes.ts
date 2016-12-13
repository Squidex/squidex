/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Pipe } from '@angular/core';

import { DateTime } from './../utils/date-time';
import { Duration } from './../utils/duration';

@Pipe({
    name: 'shortDate'
})
export class ShortDatePipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('DD.MMM');
    }
}

@Pipe({
    name: 'month'
})
export class MonthPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('MMMM');
    }
}

@Pipe({
    name: 'dayOfWeek'
})
export class DayOfWeekPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('dd');
    }
}

@Pipe({
    name: 'day'
})
export class DayPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('DD');
    }
}

@Pipe({
    name: 'shortTime'
})
export class ShortTimePipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('HH:mm');
    }
}

@Pipe({
    name: 'duration'
})
export class DurationPipe {
    public transform(value: Duration, args: string[]): any {
        return value.toString();
    }
}