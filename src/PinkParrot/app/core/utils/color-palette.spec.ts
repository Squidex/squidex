/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ColorPalette } from './../';

describe('ColorPalatte', () => {
    it('should generate colors', () => {
        const palette = ColorPalette.colors();

        expect(palette.colors.length).toBeGreaterThan(20);
    });
});
