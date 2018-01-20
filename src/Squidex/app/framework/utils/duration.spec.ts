/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime, Duration } from './../';

describe('Duration', () => {
    it('should instantiate and provide timestamp as property', () => {
        const duration = new Duration(10);

        expect(duration.timestamp).toBe(10);
    });

    it('should calculate timestamp from first and second time', () => {
        const s = DateTime.today();
        const d = s.addSeconds(100);

        const duration = Duration.create(s, d);

        const actual = duration.timestamp;
        const expected = 100000;

        expect(actual).toBe(expected);
    });

    it('should print to string correctly', () => {
        const s = DateTime.today();
        const d = s.addHours(1).addMinutes(30).addSeconds(60);

        const duration = Duration.create(s, d);

        const actual = duration.toString();
        const expected = '1:31h';

        expect(actual).toBe(expected);
    });

    it('should print to string correctly for one digit minutes', () => {
        const s = DateTime.today();
        const d = s.addHours(1).addMinutes(1).addSeconds(60);

        const duration = Duration.create(s, d);

        const actual = duration.toString();
        const expected = '1:02h';

        expect(actual).toBe(expected);
    });
});
