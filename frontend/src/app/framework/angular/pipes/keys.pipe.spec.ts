/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { KeysPipe } from './keys.pipe';

describe('KeysPipe', () => {
    it('should return keys', () => {
        const value = {
            key1: 1,
            key2: 2,
        };

        const pipe = new KeysPipe();

        const actual = pipe.transform(value);
        const expected = ['key1', 'key2'];

        expect(actual).toEqual(expected);
    });
});
