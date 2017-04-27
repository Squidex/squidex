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
    name: 'shortDate',
    pure: true
})
export class ShortDatePipe {
    public transform(value: DateTime): any {
        return value.toStringFormat('DD.MMM');
    }
}

@Pipe({
    name: 'month',
    pure: true
})
export class MonthPipe {
    public transform(value: DateTime): any {
        return value.toStringFormat('MMMM');
    }
}

@Pipe({
    name: 'fromNow',
    pure: true
})
export class FromNowPipe {
    public transform(value: DateTime): any {
        return value.toFromNow();
    }
}

@Pipe({
    name: 'dayOfWeek',
    pure: true
})
export class DayOfWeekPipe {
    public transform(value: DateTime): any {
        return value.toStringFormat('dd');
    }
}

@Pipe({
    name: 'day',
    pure: true
})
export class DayPipe {
    public transform(value: DateTime): any {
        return value.toStringFormat('DD');
    }
}

@Pipe({
    name: 'shortTime',
    pure: true
})
export class ShortTimePipe {
    public transform(value: DateTime): any {
        return value.toStringFormat('HH:mm');
    }
}

@Pipe({
    name: 'duration',
    pure: true
})
export class DurationPipe {
    public transform(value: Duration): any {
        return value.toString();
    }
}