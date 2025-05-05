/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EntriesPipe, KeysPipe, ReversePipe } from './array.pipes';

describe('KeysPipe', () => {
    const pipe = new KeysPipe();

    it('should return keys', () => {
        const value = {
            key1: 1,
            key2: 2,
        };

        const actual = pipe.transform(value);
        const expected = ['key1', 'key2'];

        expect(actual).toEqual(expected);
    });

    it('should return sorted keys', () => {
        const value = {
            key2: 2,
            key1: 1,
        };

        const actual = pipe.transform(value);
        const expected = ['key1', 'key2'];

        expect(actual).toEqual(expected);
    });
});

describe('ReversePipe', () => {
    const pipe = new ReversePipe();

    it('should reverse entries', () => {
        const value = [1, 2];

        const actual = pipe.transform(value);
        const expected = [2, 1];

        expect(actual).toEqual(expected);
    });
});


describe('EntriesPipe', () => {
    const pipe = new EntriesPipe();

    it('should get entries', () => {
        const value = {
            key1: 1,
            key2: 2,
        };

        const actual = pipe.transform(value);
        const expected = [{ key: 'key1', value: 1 }, { key: 'key2', value: 2 }];

        expect(actual).toEqual(expected);
    });

    it('should get entries sorted', () => {
        const value = {
            key2: { property: 2 },
            key1: { property: 1 },
        };

        const actual = pipe.transform(value, 'property');
        const expected = [{ key: 'key1', value: { property: 1 } }, { key: 'key2', value: { property: 2 } }];

        expect(actual).toEqual(expected);
    });
});
