/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from './date-time';
import { Duration } from './duration';

describe('Duration', () => {
    it('should instantiate and provide timestamp as property', () => {
        const duration = new Duration(10);

        expect(duration.timestamp).toBe(10);
    });

    it('should calculate timestamp from first and second time', () => {
        const time1 = DateTime.today();
        const time2 = time1.addSeconds(100);

        const duration = Duration.create(time1, time2);

        const actual = duration.timestamp;

        expect(actual).toBe(100000);
    });

    it('should print to string correctly', () => {
        const time1 = DateTime.today();
        const time2 = time1.addHours(12).addMinutes(30).addSeconds(60);

        const duration = Duration.create(time1, time2);

        const actual = duration.toString();

        expect(actual).toBe('12:31:00');
    });

    it('should print to string correctly for one digit minutes', () => {
        const time1 = DateTime.today();
        const time2 = time1.addHours(1).addMinutes(2).addSeconds(5);

        const duration = Duration.create(time1, time2);

        const actual = duration.toString();

        expect(actual).toBe('01:02:05');
    });

    it('should print to string correctly for one partial seconds', () => {
        const time1 = DateTime.today();
        const time2 = time1.addHours(1).addMinutes(2).addSeconds(4.555334);

        const duration = Duration.create(time1, time2);

        const actual = duration.toString();

        expect(actual).toBe('01:02:04');
    });
});
