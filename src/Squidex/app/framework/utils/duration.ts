/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import * as moment from 'moment';

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
        const duration = moment.duration(this.value);

        let hoursString = Math.floor(duration.asHours()).toString();

        if (hoursString.length === 1) {
            hoursString = `0${hoursString}`;
        }

        let minutesString = duration.minutes().toString();

        if (minutesString.length === 1) {
            minutesString = `0${minutesString}`;
        }

        let secondsString = duration.seconds().toString();

        if (secondsString.length === 1) {
            secondsString = `0${secondsString}`;
        }

        return `${hoursString}:${minutesString}:${secondsString}`;
    }
}