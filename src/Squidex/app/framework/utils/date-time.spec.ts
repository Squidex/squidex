/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from './date-time';

describe('DateTime', () => {
    const today = DateTime.today();
    const today2 = DateTime.today();
    const now = DateTime.now();

    it('should parse from iso string', () => {
        const value = DateTime.parseISO('2013-10-16T12:13:14.125');

        expect(value.year).toBe(2013);
        expect(value.month).toBe(10);
        expect(value.day).toBe(16);
        expect(value.hours).toBe(12);
        expect(value.minutes).toBe(13);
        expect(value.seconds).toBe(14);
        expect(value.milliseconds).toBe(125);
        expect(value.weekDay).toBe(3);

        expect(value.raw).not.toBeNull();
    });

    it('should throw when date string to parse is null', () => {
        expect(() => DateTime.parseISO('#')).toThrow();
    });

    it('should throw when date string to parse is invalid', () => {
        expect(() => DateTime.parse('#', 'yyyy-MM-dd')).toThrow();
    });

    it('should throw when utc date string to parse is invalid', () => {
        expect(() => DateTime.parseUTC('#', 'yyyy-MM-dd')).toThrow();
    });

    it('should parse Microsoft date format', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000)/');
        const expected = DateTime.parseISO('2008-10-15T04:00:00');

        expect(actual).toEqual(expected);
    });

    it('should parse Microsoft date format with positive offset', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000+2)/');
        const expected = DateTime.parseISO('2008-10-15T06:00:00');

        expect(actual).toEqual(expected);
    });

    it('should parse Microsoft date format with negative offset', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000-2)/');
        const expected = DateTime.parseISO('2008-10-15T02:00:00');

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
        const value = DateTime.parseISO('2013-10-16T12:13:14');
        const expected = '12:13';

        expect(value.toStringFormat('HH:mm')).toEqual(expected);
    });

    it('should print to iso string', () => {
        const value = DateTime.parseISO_UTC('2013-10-16T12:13:14');
        const expected = '2013-10-16T12:13:14.000Z';

        expect(value.toISOString()).toEqual(expected);
    });

    it('should print to from now string', () => {
        const value = DateTime.now().addMinutes(-4);
        const expected = '4 minutes ago';

        expect(value.toFromNow()).toBe(expected);
    });

    it('should print from format with underscore', () => {
        const actual = DateTime.parseISO('2013-10-16T00:00:00');
        const expected = DateTime.parse('10_2013_16', 'MM_YYYY_DD');

        expect(actual).toEqual(expected);
    });

    it('should calculate valid first of week', () => {
        const actual = DateTime.parseISO_UTC('2013-10-16T12:13:14.125').firstOfWeek();
        const expected = DateTime.parseISO_UTC('2013-10-14T00:00:00');

        expect(actual).toEqual(expected);
    });

    it('should calculate valid first of month', () => {
        const actual = DateTime.parseISO_UTC('2013-10-16T12:13:14.125').firstOfMonth();
        const expected = DateTime.parseISO_UTC('2013-10-01');

        expect(actual.toISOString()).toEqual(expected.toISOString());
    });

    it('should add various offsets to date time', () => {
        const actual =
            DateTime.parseISO_UTC('2013-05-01T12:12:12.100')
                .addYears(1)
                .addMonths(2)
                .addDays(13)
                .addHours(3)
                .addMinutes(10)
                .addSeconds(15)
                .addMilliseconds(125);
        const expected = DateTime.parseISO_UTC('2014-07-16T15:22:27.225');

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
});
