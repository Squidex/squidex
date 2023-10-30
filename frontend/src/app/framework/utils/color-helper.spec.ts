/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/naming-convention */

import { ColorHelper } from './color-helper';

describe('ColorHelper', () => {
    [
        { r: 1, g: 0, b: 0, h: 0, name: 'red' },
        { r: 1, g: 1, b: 0, h: 60, name: 'yellow' },
        { r: 0, g: 1, b: 0, h: 120, name: 'green' },
        { r: 0, g: 1, b: 1, h: 180, name: 'cyan' },
        { r: 0, g: 0, b: 1, h: 240, name: 'blue' },
        { r: 1, g: 0, b: 1, h: 300, name: 'pink' },
        { r: 1, g: 0, b: 0, h: 360, name: 'red2' },
    ].forEach(test => {
        it(`should convert from hsv ${test.name}`, () => {
            const color = ColorHelper.hsvToRgb({ h: test.h, s: 1, v: 1 });

            expect(color).toEqual({ r: test.r, g: test.g, b: test.b });
        });
    });

    it('should compute color from string', () => {
        const color1_1 = ColorHelper.fromStringHash('sebastian@squidex.io');
        const color1_2 = ColorHelper.fromStringHash('sebastian@squidex.io');

        const color2 = ColorHelper.fromStringHash('hello@squidex.io');

        expect(color1_1).toEqual(color1_2);
        expect(color1_1).not.toEqual(color2);
    });
});