/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime, Duration } from './../../';

import {
    DatePipe,
    DayOfWeekPipe,
    DayPipe,
    DurationPipe,
    FromNowPipe,
    FullDateTimePipe,
    ISODatePipe,
    MonthPipe,
    ShortDatePipe,
    ShortTimePipe
} from './date-time.pipes';

const dateTime = DateTime.parse('2013-10-03T12:13:14.125', DateTime.iso8601());

describe('DurationPipe', () => {
    it('should format to standard duration string', () => {
        const duration = Duration.create(dateTime, dateTime.addMinutes(10).addDays(13).addSeconds(10));

        const pipe = new DurationPipe();

        const actual = pipe.transform(duration);
        const expected = '312:10:10';

        expect(actual).toBe(expected);
    });
});

describe('FullDateTimePipe', () => {
    it('should format to nice string', () => {
        const pipe = new FullDateTimePipe();

        const actual = pipe.transform(dateTime);
        const expected = 'Thursday, October 3, 2013 12:13 PM';

        expect(actual).toBe(expected);
    });
});

describe('DayPipe', () => {
    it('should format to day numbers', () => {
        const pipe = new DayPipe();

        const actual = pipe.transform(dateTime);
        const expected = '03';

        expect(actual).toBe(expected);
    });
});

describe('MonthPipe', () => {
    it('should format to long month name', () => {
        const pipe = new MonthPipe();

        const actual = pipe.transform(dateTime);
        const expected = 'October';

        expect(actual).toBe(expected);
    });
});

describe('FromNowPipe', () => {
    it('should format to from now string', () => {
        const pipe = new FromNowPipe();

        const actual = pipe.transform(DateTime.now().addMinutes(-4));
        const expected = '4 minutes ago';

        expect(actual).toBe(expected);
    });
});

describe('DayOfWeekPipe', () => {
    it('should format to short week of day string', () => {
        const pipe = new DayOfWeekPipe();

        const actual = pipe.transform(dateTime);
        const expected = 'Th';

        expect(actual).toBe(expected);
    });
});

describe('DatePipe', () => {
    it('should format to two digit day number and short month name and year', () => {
        const pipe = new DatePipe();

        const actual = pipe.transform(dateTime);
        const expected = '03. Oct 2013';

        expect(actual).toBe(expected);
    });
});

describe('ShortDatePipe', () => {
    it('should format to two digit day number and short month name', () => {
        const pipe = new ShortDatePipe();

        const actual = pipe.transform(dateTime);
        const expected = '03. Oct';

        expect(actual).toBe(expected);
    });
});

describe('ShortTimePipe', () => {
    it('should format to short time string', () => {
        const pipe = new ShortTimePipe();

        const actual = pipe.transform(dateTime);
        const expected = '12:13';

        expect(actual).toBe(expected);
    });
});

describe('ISODatePipe', () => {
    it('should format to short time string', () => {
        const pipe = new ISODatePipe();

        const actual = pipe.transform(dateTime);
        const expected = dateTime.toISOString();

        expect(actual).toBe(expected);
    });
});