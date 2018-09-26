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
});
