/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { DateTime } from './../utils/date-time';
import { Duration } from './../utils/duration';

@Ng2.Pipe({
    name: 'shortDate'
})
export class ShortDatePipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('DD.MMM');
    }
}

@Ng2.Pipe({
    name: 'month'
})
export class MonthPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('MMMM');
    }
}

@Ng2.Pipe({
    name: 'dayOfWeek'
})
export class DayOfWeekPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('dd');
    }
}

@Ng2.Pipe({
    name: 'day'
})
export class DayPipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('DD');
    }
}

@Ng2.Pipe({
    name: 'shortTime'
})
export class ShortTimePipe {
    public transform(value: DateTime, args: string[]): any {
        return value.toStringFormat('HH:mm');
    }
}

@Ng2.Pipe({
    name: 'duration'
})
export class DurationPipe {
    public transform(value: Duration, args: string[]): any {
        return value.toString();
    }
}