/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { addDays, addHours, addMilliseconds, addMinutes, addMonths, addSeconds, addYears, format, formatDistanceToNow, formatISO, parse, parseISO, startOfDay, startOfMonth, startOfTomorrow, startOfWeek, startOfYesterday } from 'date-fns';
import { DateHelper } from './date-helper';
import { Types } from './types';

const DATE_FORMAT = 'yyyy-MM-dd';

export class DateTime {
    public get raw(): Date {
        return this.value;
    }

    public get weekDay(): number {
        return this.value.getDay();
    }

    public get year(): number {
        return this.value.getFullYear();
    }

    public get timestamp(): number {
        return this.value.getTime();
    }

    public get month(): number {
        return this.value.getMonth() + 1;
    }

    public get day(): number {
        return this.value.getDate();
    }

    public get hours(): number {
        return this.value.getHours();
    }

    public get minutes(): number {
        return this.value.getMinutes();
    }

    public get seconds(): number {
        return this.value.getSeconds();
    }

    public get milliseconds(): number {
        return this.value.getMilliseconds();
    }

    public get date(): DateTime {
        return new DateTime(startOfDay(this.value));
    }

    constructor(private readonly value: Date) {
        Object.freeze(this);
    }

    public static now(): DateTime {
        return new DateTime(new Date());
    }

    public static today(): DateTime {
        return new DateTime(startOfDay(new Date()));
    }

    public static tomorrow(): DateTime {
        return new DateTime(startOfTomorrow());
    }

    public static yesterday(): DateTime {
        return new DateTime(startOfYesterday());
    }

    public static parseISO(value: string, assumeUtc = true): DateTime {
        const result = DateTime.tryParseISO(value, assumeUtc);

        if (!result) {
            throw new Error(`${value} is not a valid datetime.`);
        }

        return result;
    }

    public static tryParseISO(value: string, assumeUtc = true): DateTime | null {
        if (!value) {
            return null;
        }

        let date: Date;

        if (value.length === DATE_FORMAT.length) {
            date = parse(value, DATE_FORMAT, new Date());
        } else {
            date = parseISO(value);
        }

        const time = date.getTime();

        if (Number.isNaN(time) || !Types.isNumber(time)) {
            return null;
        }

        if (assumeUtc && (value.length === DATE_FORMAT.length || !value.endsWith('Z'))) {
            date = DateHelper.getLocalDate(date);
        }

        return new DateTime(date);
    }

    public eq(v: DateTime): boolean {
        return v && (this === v || this.timestamp === v.timestamp);
    }

    public ne(v: DateTime): boolean {
        return !v || this.timestamp !== v.timestamp;
    }

    public lt(v: DateTime): boolean {
        return v && this.timestamp < v.timestamp;
    }

    public le(v: DateTime): boolean {
        return v && this.timestamp <= v.timestamp;
    }

    public gt(v: DateTime): boolean {
        return v && this.timestamp > v.timestamp;
    }

    public ge(v: DateTime): boolean {
        return v && this.timestamp >= v.timestamp;
    }

    public firstOfWeek(): DateTime {
        return new DateTime(startOfWeek(this.value, { weekStartsOn: 1 }));
    }

    public firstOfMonth(): DateTime {
        return new DateTime(startOfMonth(this.value));
    }

    public addYears(value: number): DateTime {
        return new DateTime(addYears(this.value, value));
    }

    public addMonths(value: number): DateTime {
        return new DateTime(addMonths(this.value, value));
    }

    public addDays(value: number): DateTime {
        return new DateTime(addDays(this.value, value));
    }

    public addHours(value: number): DateTime {
        return new DateTime(addHours(this.value, value));
    }

    public addMinutes(value: number): DateTime {
        return new DateTime(addMinutes(this.value, value));
    }

    public addSeconds(value: number): DateTime {
        return new DateTime(addSeconds(this.value, value));
    }

    public addMilliseconds(value: number): DateTime {
        return new DateTime(addMilliseconds(this.value, value));
    }

    public toISODateUTC(): string {
        return format(DateHelper.getUTCDate(this.value), DATE_FORMAT);
    }

    public toISODate(): string {
        return format(this.value, DATE_FORMAT);
    }

    public toISODateTime(): string {
        return formatISO(this.value);
    }

    public toStringFormat(pattern: string): string {
        return format(this.value, pattern, { locale: DateHelper.getFnsLocale() });
    }

    public toStringFormatUTC(pattern: string): string {
        return format(DateHelper.getUTCDate(this.value), pattern, { locale: DateHelper.getFnsLocale() });
    }

    public toFromNow(): string {
        return formatDistanceToNow(this.value, { locale: DateHelper.getFnsLocale(), addSuffix: true });
    }

    public toISOString(withoutMilliseconds = true): string {
        let result = this.value.toISOString();

        if (withoutMilliseconds) {
            result = `${result.slice(0, 19)}Z`;
        }

        return result;
    }
}
