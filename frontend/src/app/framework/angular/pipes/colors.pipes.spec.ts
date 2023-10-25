/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/naming-convention */

import { DarkenPipe, LightenPipe, StringColorPipe } from './colors.pipes';

describe('DarkenPipe', () => {
    const pipe = new DarkenPipe();

    it('should keep black unchanged', () => {
        const result = pipe.transform('#000', 20);

        expect(result).toEqual('#000000');
    });

    it('should darken rgb gray', () => {
        const result = pipe.transform('rgb(100, 100, 100)', 20);

        expect(result).toEqual('#505050');
    });

    it('should darken hey gray', () => {
        const result = pipe.transform('#646464', 20);

        expect(result).toEqual('#505050');
    });

    it('should darken mixed color', () => {
        const result = pipe.transform('#FF91D1', 20);

        expect(result).toEqual('#cc74a7');
    });
});

describe('LightenPipe', () => {
    const pipe = new LightenPipe();

    it('should keep white unchanged', () => {
        const result = pipe.transform('#fff', 20);

        expect(result).toEqual('#ffffff');
    });

    it('should lighten rgb gray', () => {
        const result = pipe.transform('rgb(100, 100, 100)', 20);

        expect(result).toEqual('#787878');
    });

    it('should lighten hey gray', () => {
        const result = pipe.transform('#646464', 20);

        expect(result).toEqual('#787878');
    });

    it('should lighten mixed color', () => {
        const result = pipe.transform('#7F4868', 20);

        expect(result).toEqual('#98567d');
    });
});

describe('StringColorPipe', () => {
    const pipe = new StringColorPipe();

    it('should compute color from string', () => {
        const color1_1 = pipe.transform('sebastian@squidex.io');
        const color1_2 = pipe.transform('sebastian@squidex.io');

        const color2 = pipe.transform('hello@squidex.io');

        expect(color1_1).toEqual(color1_2);
        expect(color1_1).not.toEqual(color2);
    });
});
