/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as moment from 'moment';

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
        const clone = this.cloneDate();

        clone.setHours(0, 0, 0, 0);

        return new DateTime(clone);
    }

    constructor(private readonly value: Date) {
        Object.freeze(this);
    }

    public static iso8601(): any {
        return moment.ISO_8601;
    }

    public static now(): DateTime {
        return new DateTime(new Date());
    }

    public static today(): DateTime {
        return DateTime.now().date;
    }

    public static tomorrow(): DateTime {
        return DateTime.now().addDays(1).date;
    }

    public static yesterday(): DateTime {
        return DateTime.now().addDays(-1).date;
    }

    public static parseMSDate(value: string): DateTime {
        let off = parseInt(value.substr(19, 3), 10);

        if (isNaN(off)) {
            off = 0;
        }

        const date = new Date(parseInt(value.substr(6), 10));
        const time = (date.getTime());
        const offs = (date.getTimezoneOffset() + off * 60) * 60000;

        date.setTime(time + offs);

        return new DateTime(date);
    }

    public static parse(value: string, format: string): DateTime {
        const parsedMoment = moment(value, format);

        if (parsedMoment.isValid()) {
            return new DateTime(parsedMoment.toDate());
        } else {
            throw `${value} is not a valid date time string`;

        }
    }

    public static parseUTC(value: string, format: string): DateTime {
        const parsedMoment = moment.utc(value, format);

        if (parsedMoment.isValid()) {
            return new DateTime(new Date(parsedMoment.valueOf() - parsedMoment.local().utcOffset() * 60 * 1000));
        } else {
            throw `${value} is not a valid date time string`;
        }
    }

    private cloneDate(): Date {
        return new Date(this.value.getTime());
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
        const weekStart = new Date(this.value.valueOf() - (this.weekDay - 1) * 86400000);

        return new DateTime(weekStart);
    }

    public firstOfMonth(): DateTime {
        const monthStart = new Date(this.year, this.month - 1, 1);

        return new DateTime(monthStart);
    }

    public addYears(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setFullYear(clone.getFullYear() + value, clone.getMonth(), clone.getDay());

        return new DateTime(clone);
    }

    public addMonths(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setMonth(clone.getMonth() + value, clone.getDate());

        return new DateTime(clone);
    }

    public addDays(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setDate(clone.getDate() + value);

        return new DateTime(clone);
    }

    public addHours(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setTime(clone.getTime() + (value * 60 * 60 * 1000));

        return new DateTime(clone);
    }

    public addMinutes(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setTime(clone.getTime() + (value * 60 * 1000));

        return new DateTime(clone);
    }

    public addSeconds(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setTime(clone.getTime() + (value * 1000));

        return new DateTime(clone);
    }

    public addMilliseconds(value: number): DateTime {
        const clone = this.cloneDate();

        clone.setTime(clone.getTime() + value);

        return new DateTime(clone);
    }

    public toUTCString(): string {
        return this.value.toUTCString();
    }

    public toString(): string {
        return moment(this.value).format();
    }

    public toStringFormat(format: string): string {
        return moment(this.value).format(format);
    }
}