/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { DateTime } from './../';

describe('DateTime', () => {
    const today = DateTime.today();
    const today2 = DateTime.today();
    const now = DateTime.now();

    it('should parse from iso string', () => {
        const value = DateTime.parse('2013-10-16T12:13:14.125', DateTime.iso8601());

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
        expect(() => DateTime.parseUTC('#', DateTime.iso8601())).toThrow();
    });

    it('should throw when date string to parse is invalid', () => {
        expect(() => DateTime.parse('#', 'yyyy-MM-dd')).toThrow();
    });

    it('should parse Microsoft date format', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000)/');
        const expected = DateTime.parse('2008-10-15T04:00:00', DateTime.iso8601());

        expect(actual).toEqual(expected);
    });

    it('should parse Microsoft date format with positive offset', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000+2)/');
        const expected = DateTime.parse('2008-10-15T06:00:00', DateTime.iso8601());

        expect(actual).toEqual(expected);
    });

    it('should parse Microsoft date format with negative offset', () => {
        const actual = DateTime.parseMSDate('/Date(1224043200000-2)/');
        const expected = DateTime.parse('2008-10-15T02:00:00', DateTime.iso8601());

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
        const value = DateTime.parseUTC('2013-10-16T12:13:14T', DateTime.iso8601());
        const actual = value.toStringFormat('hh:mm');
        const expected = '12:13';
        
        expect(actual).toEqual(expected);
    });

    it('should print to iso string', () => {
        const value = DateTime.parseUTC('2013-10-16T12:13:14', DateTime.iso8601());
        const actual = value.toString().substr(0, 19);
        const expected = '2013-10-16T12:13:14';
        
        expect(actual).toEqual(expected);
    });

    it('should print to valid utc string', () => {
        const value = DateTime.parseUTC('2013-10-16T12:13:14', DateTime.iso8601());

        expect(value.toUTCString()).toBeDefined();
    });

    it('should print from format with underscore', () => {
        const actual = DateTime.parse('2013-10-16T00:00:00', DateTime.iso8601());
        const expected = DateTime.parse('10_2013_16', 'MM_YYYY_DD');

        expect(actual).toEqual(expected);
    });

    it('should calculate valid first of week', () => {
        const actual = DateTime.parseUTC('2013-10-16T12:13:14.125', DateTime.iso8601()).date.firstOfWeek();
        const expected = DateTime.parseUTC('2013-10-14T00:00:00', DateTime.iso8601());

        expect(actual).toEqual(expected);
    });

    it('should calculate valid first of month', () => {
        const actual = DateTime.parseUTC('2013-10-16T12:13:14.125', DateTime.iso8601()).date.firstOfMonth();
        const expected = DateTime.parseUTC('2013-10-01', DateTime.iso8601());

        expect(actual).toEqual(expected);
    });

    it('should add various offsets to date time', () => {
        const actual =
            DateTime.parseUTC('2013-05-01T12:12:12.100', DateTime.iso8601())
                .addYears(1)
                .addMonths(2)
                .addDays(13)
                .addHours(3)
                .addMinutes(10)
                .addSeconds(15)
                .addMilliseconds(125);
        const expected = DateTime.parseUTC('2014-07-16T15:22:27.225', DateTime.iso8601());

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
