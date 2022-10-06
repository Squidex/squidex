/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateHelper } from '..';
import { DateTime } from './date-time';

describe('DateTime', () => {
    const today = DateTime.today();
    const today2 = DateTime.today();
    const now = DateTime.now();

    beforeEach(() => {
        DateHelper.setlocale(null);
    });

    it('should parse from iso string', () => {
        const actual = DateTime.parseISO('2013-10-16T12:13:14.125', false);

        expect(actual.year).toBe(2013);
        expect(actual.month).toBe(10);
        expect(actual.day).toBe(16);
        expect(actual.hours).toBe(12);
        expect(actual.minutes).toBe(13);
        expect(actual.seconds).toBe(14);
        expect(actual.milliseconds).toBe(125);
        expect(actual.weekDay).toBe(3);

        expect(actual.raw).not.toBeNull();
    });

    it('should throw if date string to parse is null', () => {
        expect(() => DateTime.parseISO(null!)).toThrow();
    });

    it('should throw if date string to parse is invalid', () => {
        expect(() => DateTime.parseISO('#')).toThrow();
    });

    it('should return null if date string to try parse is null', () => {
        expect(DateTime.tryParseISO(null!)).toBeNull();
    });

    it('should return null if date string to try parse is invalid', () => {
        expect(DateTime.tryParseISO(null!)).toBeNull();
    });

    it('should parse date from utc date', () => {
        const actual = DateTime.parseISO('2013-10-16');
        const expected = DateTime.parseISO('2013-10-16T00:00:00Z');

        expect(actual).toEqual(expected);
    });

    it('should create today and now instance correctly', () => {
        const actual = DateTime.today();
        const expected = DateTime.now().date;

        expect(actual).toEqual(expected);
    });

    it('should create tomorrow instance correctly', () => {
        const actual = DateTime.tomorrow();
        const expected = DateTime.today().addDays(1);

        expect(actual).toEqual(expected);
    });

    it('should create yesterday instance correctly', () => {
        const actual = DateTime.yesterday();
        const expected = DateTime.today().addDays(-1);

        expect(actual).toEqual(expected);
    });

    it('should print to formatted string', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14', false);
        const expected = '12:13';

        expect(value.toStringFormat('HH:mm')).toEqual(expected);
    });

    it('should print to formatted ISO string', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14.123Z');
        const expected = '12:13';

        expect(value.toStringFormatUTC('HH:mm')).toEqual(expected);
    });

    it('should print to iso string', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14.123Z');
        const expected = '2013-10-16T12:13:14Z';

        expect(value.toISOString()).toEqual(expected);
    });

    it('should print to iso string with milliseconds', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14.123Z');
        const expected = '2013-10-16T12:13:14.123Z';

        expect(value.toISOString(false)).toEqual(expected);
    });

    it('should print to iso date', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14Z');
        const expected = '2013-10-16';

        expect(value.toISODate()).toEqual(expected);
    });

    it('should print to iso utc date', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14Z');
        const expected = '2013-10-16';

        expect(value.toISODate()).toEqual(expected);
    });

    it('should print to from now string', () => {
        const value = DateTime.now().addMinutes(-4);
        const expected = '4 minutes ago';

        expect(value.toFromNow()).toBe(expected);
    });

    it('should calculate valid first of week', () => {
        const actual = DateTime.parseISO('2013-10-16T12:13:14.125', false).firstOfWeek();
        const expected = DateTime.parseISO('2013-10-14', false);

        expect(actual).toEqual(expected);
    });

    it('should calculate valid first of month', () => {
        const actual = DateTime.parseISO('2013-10-16T12:13:14.125', false).firstOfMonth();
        const expected = DateTime.parseISO('2013-10-01', false);

        expect(actual.toISOString()).toEqual(expected.toISOString());
    });

    it('should add years to date time', () => {
        const actual = DateTime.parseISO('2013-01-01T12:12:12.100Z').addYears(2);
        const expected = DateTime.parseISO('2015-01-01T12:12:12.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add months to date time', () => {
        const actual = DateTime.parseISO('2015-01-01T12:12:12.100Z').addMonths(1);
        const expected = DateTime.parseISO('2015-02-01T12:12:12.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add days to date time', () => {
        const actual = DateTime.parseISO('2015-02-01T12:12:12.100Z').addDays(9);
        const expected = DateTime.parseISO('2015-02-10T12:12:12.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add hours to date time', () => {
        const actual = DateTime.parseISO('2015-02-10T12:12:12.100Z').addHours(11);
        const expected = DateTime.parseISO('2015-02-10T23:12:12.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add minutes to date time', () => {
        const actual = DateTime.parseISO('2015-02-10T23:12:12.100Z').addMinutes(7);
        const expected = DateTime.parseISO('2015-02-10T23:19:12.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add seconds to date time', () => {
        const actual = DateTime.parseISO('2015-02-10T23:19:12.100Z').addSeconds(5);
        const expected = DateTime.parseISO('2015-02-10T23:19:17.100Z');

        expect(actual).toEqual(expected);
    });

    it('should add milliseconds to date time', () => {
        const actual = DateTime.parseISO('2015-02-10T23:19:17.100Z').addMilliseconds(125);
        const expected = DateTime.parseISO('2015-02-10T23:19:17.225Z');

        expect(actual).toEqual(expected);
    });

    it('should make valid equal comparisons', () => {
        expect(today.eq(today2)).toBeTruthy();
        expect(today.eq(now)).toBeFalsy();
    });

    it('should make valid not equal comparisons', () => {
        expect(today.ne(today2)).toBeFalsy();
        expect(today.ne(now)).toBeTruthy();
    });

    it('should make valid less comparisons', () => {
        expect(today.lt(now)).toBeTruthy();
        expect(now.lt(today)).toBeFalsy();
    });

    it('should make valid less equals comparisons', () => {
        expect(today.le(now)).toBeTruthy();
        expect(today.le(today)).toBeTruthy();
        expect(now.le(today)).toBeFalsy();
    });

    it('should make valid greater comparisons', () => {
        expect(now.gt(today)).toBeTruthy();
        expect(today.gt(now)).toBeFalsy();
    });

    it('should make valid greater equals comparisons', () => {
        expect(now.ge(today)).toBeTruthy();
        expect(now.ge(now)).toBeTruthy();
        expect(today.ge(now)).toBeFalsy();
    });

    describe('for Dutch locale', () => {
        beforeEach(() => {
            DateHelper.setlocale('nl');
        });

        afterEach(() => {
            DateHelper.setlocale(null);
        });

        it('should format to from now string', () => {
            const value = DateTime.now().addMinutes(-4);
            const expected = '4 minuten geleden';

            expect(value.toFromNow()).toBe(expected);
        });

        it('should format to string', () => {
            const value = DateTime.parseISO('2020-07-23');
            const expected = 'donderdag 23 juli 2020';

            expect(value.toStringFormat('PPPP')).toBe(expected);
        });

        it('should format to UTC string', () => {
            const value = DateTime.parseISO('2020-05-23T12:00');
            const expected = '23 mei 2020 om 12:00';

            expect(value.toStringFormatUTC('PPPp')).toBe(expected);
        });
    });

    describe('for Italian locale', () => {
        beforeEach(() => {
            DateHelper.setlocale('it');
        });

        afterEach(() => {
            DateHelper.setlocale(null);
        });

        it('should format to from now string', () => {
            const value = DateTime.now().addMinutes(-4);
            const expected = '4 minuti fa';

            expect(value.toFromNow()).toBe(expected);
        });

        it('should format to string', () => {
            const value = DateTime.parseISO('2020-07-23');
            const expected = 'giovedì 23 luglio 2020';

            expect(value.toStringFormat('PPPP')).toBe(expected);
        });

        it('should format to UTC string', () => {
            const value = DateTime.parseISO('2020-05-23T12:00');
            const expected = '23 maggio 2020 12:00';

            expect(value.toStringFormatUTC('PPPp')).toBe(expected);
        });
    });
});
