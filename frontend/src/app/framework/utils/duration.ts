/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from './date-time';

export class Duration {
    public get timestamp(): number {
        return this.value;
    }

    constructor(private readonly value: number) {
        Object.freeze(this);
    }

    public static create(first: DateTime, second: DateTime): Duration {
        return new Duration(second.timestamp - first.timestamp);
    }

    public toString(): string {
        let seconds = this.value / 1000;

        const hours = Math.floor(seconds / 3600);

        let hoursString = hours.toString();

        if (hoursString.length === 1) {
            hoursString = `0${hoursString}`;
        }

        seconds %= 3600;

        const minutes = Math.floor(seconds / 60);

        let minutesString = minutes.toString();

        if (minutesString.length === 1) {
            minutesString = `0${minutesString}`;
        }

        seconds %= 60;

        let secondsString = Math.ceil(seconds).toString();

        if (secondsString.length === 1) {
            secondsString = `0${secondsString}`;
        }

        return `${hoursString}:${minutesString}:${secondsString}`;
    }
}
