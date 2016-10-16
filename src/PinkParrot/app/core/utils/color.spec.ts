/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Color } from './../';

describe('Color', () => {
    it('should instantiate', () => {
        const color = new Color(0.3, 0.6, 0.9);

        expect(color.r).toBe(0.3);
        expect(color.g).toBe(0.6);
        expect(color.b).toBe(0.9);
    });

    it('should adjust values when instantiating', () => {
        const color = new Color(-1, 0.5, 2);

        expect(color.r).toBe(0);
        expect(color.g).toBe(0.5);
        expect(color.b).toBe(1);
    });

    it('should convert to color', () => {
        const color = Color.fromHex(23, 46, 59);

        expect(color.toNumber()).toBe(0x234659);
    });

    it('should convert to string', () => {
        const color = Color.fromHex(23, 46, 59);

        expect(color.toString()).toBe('#234659');
    });

    it('should convert to string with leading zeros', () => {
        const color = Color.fromHex(3, 6, 9);

        expect(color.toString()).toBe('#030609');
    });

    it('should be created from long string', () => {
        const color = Color.fromValue('#336699');

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
    });

    it('should be created from short string', () => {
        const color = Color.fromValue('#369');

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
    });

    it('should be created from rgb string', () => {
        const color = Color.fromValue('rgb(51, 102, 153)');

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
    });

    it('should be created from rgb number', () => {
        const color = Color.fromValue(0x336699);

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
    });

    it('should be created from rgb values', () => {
        const color = Color.fromHex(33, 66, 99);

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
    });

    it('should convert from hsl with black', () => {
        const color = Color.fromHsl(0, 0, 0);

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsl with dark black', () => {
        const color = Color.fromHsl(0, 1, 0.1);

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsl with red', () => {
        const color = Color.fromHsl(0, 1, 0.5);

        expect(color.r).toBeCloseTo(1, 1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsl with yellow', () => {
        const color = Color.fromHsl(60, 1, 0.5);

        expect(color.r).toBeCloseTo(1, 1);
        expect(color.g).toBeCloseTo(1, 1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsl with blue', () => {
        const color = Color.fromHsl(120, 1, 0.5);

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsl with turkis', () => {
        const color = Color.fromHsl(180, 1, 0.5);

        expect(color.r).toBe(0);
        expect(color.g).toBeCloseTo(1, 1);
        expect(color.b).toBeCloseTo(1, 1);
    });

    it('should convert from hsv with red', () => {
        const color = Color.fromHsv(240, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should convert from hsv with pink', () => {
        const color = Color.fromHsv(300, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should convert from hsl with another red', () => {
        const color = Color.fromHsl(360, 1, 0.5);

        expect(color.r).toBeCloseTo(1, 1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with red', () => {
        const color = Color.fromHsv(0, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with yellow', () => {
        const color = Color.fromHsv(60, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with blue', () => {
        const color = Color.fromHsv(120, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with turkis', () => {
        const color = Color.fromHsv(180, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(1);
    });

    it('should convert from hsv with red', () => {
        const color = Color.fromHsv(240, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should convert from hsv with pink', () => {
        const color = Color.fromHsv(300, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should be valid black', () => {
        const color = Color.BLACK;

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should be valid white', () => {
        const color = Color.WHITE;

        expect(color.r).toBe(1);
        expect(color.g).toBe(1);
        expect(color.b).toBe(1);
    });

    it('should be valid red', () => {
        const color = Color.RED;

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should be valid green', () => {
        const color = Color.GREEN;

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should be valid blue', () => {
        const color = Color.BLUE;

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should calculate correct luminance', () => {
        expect(new Color(1, 1, 1).luminance).toBe(1);
        expect(new Color(0, 0, 0).luminance).toBe(0);
        expect(new Color(1, 0, 0).luminance).toBe(1 / 3);
        expect(new Color(0, 1, 0).luminance).toBe(1 / 2);
        expect(new Color(0, 0, 1).luminance).toBe(1 / 6);
    });

    it('should make valid equal comparisons', () => {
        expect(new Color(0.1, 0.1, 0.1).eq(new Color(0.1, 0.1, 0.1))).toBeTruthy();
        expect(new Color(0.1, 0.1, 0.4).eq(new Color(0.1, 0.1, 0.1))).toBeFalsy();
    });

    it('should make valid not equal comparisons', () => {
        expect(new Color(0.1, 0.1, 0.1).ne(new Color(0.1, 0.1, 0.4))).toBeTruthy();
        expect(new Color(0.1, 0.1, 0.1).ne(new Color(0.1, 0.1, 0.1))).toBeFalsy();
    });

    it('should return color when creating from color', () => {
        const color = Color.fromHsv(300, 1, 1);
        const created = Color.fromValue(color);

        expect(created).toBe(color);
    });

    it('should throw error for invalid string', () => {
        expect(() => Color.fromValue('INVALID')).toThrowError('Color is not in a valid format.');
    });
});
