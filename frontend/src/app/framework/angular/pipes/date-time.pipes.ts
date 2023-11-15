/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { DateTime, Duration, Types } from '@app/framework/internal';

@Pipe({
    name: 'sqxShortDate',
    pure: true,
    standalone: true,
})
export class ShortDatePipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('dd. MMM');
    }
}

@Pipe({
    name: 'sqxISODate',
    pure: true,
    standalone: true,
})
export class ISODatePipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toISOString();
    }
}

@Pipe({
    name: 'sqxDate',
    pure: true,
    standalone: true,
})
export class DatePipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('dd. LLL yyyy');
    }
}

@Pipe({
    name: 'sqxMonth',
    pure: true,
    standalone: true,
})
export class MonthPipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('LLLL');
    }
}

@Pipe({
    name: 'sqxFromNow',
    pure: true,
    standalone: true,
})
export class FromNowPipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toFromNow();
    }
}

@Pipe({
    name: 'sqxDayOfWeek',
    pure: true,
    standalone: true,
})
export class DayOfWeekPipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('E');
    }
}

@Pipe({
    name: 'sqxDay',
    pure: true,
    standalone: true,
})
export class DayPipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('dd');
    }
}

@Pipe({
    name: 'sqxShortTime',
    pure: true,
    standalone: true,
})
export class ShortTimePipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('HH:mm');
    }
}

@Pipe({
    name: 'sqxFullDateTime',
    pure: true,
    standalone: true,
})
export class FullDateTimePipe implements PipeTransform {
    public transform(value: DateTime | string | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        if (Types.isString(value)) {
            value = DateTime.parseISO(value);
        }

        return value.toStringFormat('PPpp');
    }
}

@Pipe({
    name: 'sqxDuration',
    pure: true,
    standalone: true,
})
export class DurationPipe implements PipeTransform {
    public transform(value: Duration | undefined | null, fallback = ''): string {
        if (!value) {
            return fallback;
        }

        return value.toString();
    }
}
