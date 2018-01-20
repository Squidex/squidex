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

        let minutesString = duration.minutes().toString();

        if (minutesString.length === 1) {
            minutesString = `0${minutesString}`;
        }

        return Math.floor(duration.asHours()) + ':' + minutesString + 'h';
    }
}