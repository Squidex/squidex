/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import { DateTime, Duration } from '@app/framework/internal';

@Pipe({
    name: 'sqxShortDate',
    pure: true
})
export class ShortDatePipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('DD. MMM');
    }
}

@Pipe({
    name: 'sqxISODate',
    pure: true
})
export class ISODatePipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toISOString();
    }
}

@Pipe({
    name: 'sqxDate',
    pure: true
})
export class DatePipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('DD. MMM YYYY');
    }
}

@Pipe({
    name: 'sqxMonth',
    pure: true
})
export class MonthPipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('MMMM');
    }
}

@Pipe({
    name: 'sqxFromNow',
    pure: true
})
export class FromNowPipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toFromNow();
    }
}

@Pipe({
    name: 'sqxDayOfWeek',
    pure: true
})
export class DayOfWeekPipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('dd');
    }
}

@Pipe({
    name: 'sqxDay',
    pure: true
})
export class DayPipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('DD');
    }
}

@Pipe({
    name: 'sqxShortTime',
    pure: true
})
export class ShortTimePipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('HH:mm');
    }
}

@Pipe({
    name: 'sqxFullDateTime',
    pure: true
})
export class FullDateTimePipe implements PipeTransform {
    public transform(value: DateTime | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toStringFormat('LLL');
    }
}

@Pipe({
    name: 'sqxDuration',
    pure: true
})
export class DurationPipe implements PipeTransform {
    public transform(value: Duration | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toString();
    }
}