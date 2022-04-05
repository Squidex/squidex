/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { MathHelper } from './math-helper';

describe('MathHelper', () => {
    it('should calculate same crc32 for same input', () => {
        const crc1 = MathHelper.crc32('input');
        const crc2 = MathHelper.crc32('input');

        expect(crc1).toBe(crc2);
    });

    it('should calculate different crc32 for different input', () => {
        const crc1 = MathHelper.crc32('input1');
        const crc2 = MathHelper.crc32('input2');

        expect(crc1).not.toBe(crc2);
    });

    it('should calculate different ids', () => {
        const guid1 = MathHelper.guid();
        const guid2 = MathHelper.guid();

        expect(guid1).not.toBe(guid2);
    });

    it('should convert to rad', () => {
        expect(MathHelper.toRad(0)).toBe(0);
        expect(MathHelper.toRad(180)).toBe(Math.PI * 1);
        expect(MathHelper.toRad(360)).toBe(Math.PI * 2);
    });

    it('should convert to degree', () => {
        expect(MathHelper.toDegree(0)).toBe(0);
        expect(MathHelper.toDegree(Math.PI * 1)).toBe(180);
        expect(MathHelper.toDegree(Math.PI * 2)).toBe(360);
    });

    it('should adjust invalid degrees', () => {
        expect(MathHelper.toPositiveDegree(36.5 - (1 * 360))).toBe(36.5);
        expect(MathHelper.toPositiveDegree(36.5 - (2 * 360))).toBe(36.5);
        expect(MathHelper.toPositiveDegree(36.5 + (1 * 360))).toBe(36.5);
        expect(MathHelper.toPositiveDegree(36.5 + (2 * 360))).toBe(36.5);
    });

    it('should calculate simple sin', () => {
        expect(MathHelper.simpleSin(0)).toBe(0);
        expect(MathHelper.simpleSin(90)).toBe(1);
        expect(MathHelper.simpleSin(180)).toBe(0);
        expect(MathHelper.simpleSin(270)).toBe(1);
    });

    it('should calculate simple cos', () => {
        expect(MathHelper.simpleCos(0)).toBe(1);
        expect(MathHelper.simpleCos(90)).toBe(0);
        expect(MathHelper.simpleCos(180)).toBe(1);
        expect(MathHelper.simpleCos(270)).toBe(0);
    });

    it('should calculate multiple of 10', () => {
        expect(MathHelper.roundToMultipleOf(13, 10)).toBe(10);
        expect(MathHelper.roundToMultipleOf(16, 10)).toBe(20);
    });

    it('should calculate multiple of 2', () => {
        expect(MathHelper.roundToMultipleOfTwo(13)).toBe(14);
        expect(MathHelper.roundToMultipleOfTwo(12.2)).toBe(12);
    });

    it('should create color from long string', () => {
        const color = MathHelper.parseColor('#336699')!;

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
        expect(color.a).toBe(1.0);
    });

    it('should create color from short string', () => {
        const color = MathHelper.parseColor('#369')!;

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
        expect(color.a).toBe(1.0);
    });

    it('should create color from rgb string', () => {
        const color = MathHelper.parseColor('rgb(51, 102, 153)')!;

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
        expect(color.a).toBe(1.0);
    });

    it('should create color from rgba string', () => {
        const color = MathHelper.parseColor('rgba(51, 102, 153, 0.5)')!;

        expect(color.r).toBe(0.2);
        expect(color.g).toBe(0.4);
        expect(color.b).toBe(0.6);
        expect(color.a).toBe(0.5);
    });

    it('should convert from hsv with red', () => {
        const color = MathHelper.colorFromHsv(0, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with yellow', () => {
        const color = MathHelper.colorFromHsv(60, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with blue', () => {
        const color = MathHelper.colorFromHsv(120, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(0);
    });

    it('should convert from hsv with turkis', () => {
        const color = MathHelper.colorFromHsv(180, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(1);
        expect(color.b).toBe(1);
    });

    it('should convert from hsv with red', () => {
        const color = MathHelper.colorFromHsv(240, 1, 1);

        expect(color.r).toBe(0);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should convert from hsv with pink', () => {
        const color = MathHelper.colorFromHsv(300, 1, 1);

        expect(color.r).toBe(1);
        expect(color.g).toBe(0);
        expect(color.b).toBe(1);
    });

    it('should convert to luminance', () => {
        expect(MathHelper.toLuminance(undefined!)).toBe(1);

        expect(MathHelper.toLuminance({ r: 0, g: 0, b: 0, a: 1 })).toBe(0);
        expect(MathHelper.toLuminance({ r: 1, g: 1, b: 1, a: 1 })).toBe(1);

        expect(MathHelper.toLuminance({ r: 0.5, g: 0.5, b: 0.5, a: 1 })).toBe(0.5);
    });

    it('should generate random color', () => {
        const color = MathHelper.randomColor();

        expect(color[0]).toEqual('#');
    });
});
