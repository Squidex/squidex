/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Rotation, Vec2 } from './../';

describe('Vec2', () => {
    it('should instantiate from x and y value', () => {
        const v = new Vec2(10, 20);

        expect(v.x).toBe(10);
        expect(v.y).toBe(20);

        expect(v.toString()).toBe('(10, 20)');
    });

    it('should make valid equal comparisons', () => {
        expect(new Vec2(10, 10).eq(new Vec2(10, 10))).toBeTruthy();
        expect(new Vec2(10, 10).eq(new Vec2(20, 20))).toBeFalsy();
    });

    it('should make valid not equal comparisons', () => {
        expect(new Vec2(10, 10).ne(new Vec2(20, 20))).toBeTruthy();
        expect(new Vec2(10, 10).ne(new Vec2(10, 10))).toBeFalsy();
    });

    it('should calculate multiple of 10', () => {
        const actual = new Vec2(13, 16).round(10);
        const expected = new Vec2(10, 20);

        expect(actual).toEqual(expected);
    });

    it('should calculate multiple of 2', () => {
        const actual = new Vec2(13, 12.2).roundToMultipleOfTwo();
        const expected = new Vec2(14, 12);

        expect(actual).toEqual(expected);
    });

    it('should add by vector correctly', () => {
        const actual = new Vec2(15, 12).add(new Vec2(4, 1));
        const expected = new Vec2(19, 13);

        expect(actual).toEqual(expected);
    });

    it('should add by scalar correctly', () => {
        const actual = new Vec2(15, 12).addScalar(3);
        const expected = new Vec2(18, 15);

        expect(actual).toEqual(expected);
    });

    it('should subtract by vector correctly', () => {
        const actual = new Vec2(15, 12).sub(new Vec2(4, 1));
        const expected = new Vec2(11, 11);

        expect(actual).toEqual(expected);
    });

    it('should subtract by scalar correctly', () => {
        const actual = new Vec2(15, 12).subScalar(3);
        const expected = new Vec2(12, 9);

        expect(actual).toEqual(expected);
    });

    it('should multiply by vector correctly', () => {
        const actual = new Vec2(15, 12).mul(new Vec2(4, 2));
        const expected = new Vec2(60, 24);

        expect(actual).toEqual(expected);
    });

    it('should multiply by scalar correctly', () => {
        const actual = new Vec2(15, 12).mulScalar(3);
        const expected = new Vec2(45, 36);

        expect(actual).toEqual(expected);
    });

    it('should divide by vector correctly', () => {
        const actual = new Vec2(15, 12).div(new Vec2(5, 2));
        const expected = new Vec2(3, 6);

        expect(actual).toEqual(expected);
    });

    it('should divide by scalar correctly', () => {
        const actual = new Vec2(15, 12).divScalar(3);
        const expected = new Vec2(5, 4);

        expect(actual).toEqual(expected);
    });

    it('should negate correctly', () => {
        const actual = new Vec2(15, 12).negate();
        const expected = new Vec2(-15, -12);

        expect(actual).toEqual(expected);
    });

    it('should calculate max vector', () => {
        const actual = Vec2.max(new Vec2(20, 10), new Vec2(15, 30));
        const expected = new Vec2(20, 30);

        expect(actual).toEqual(expected);
    });

    it('should calculate min vector', () => {
        const actual = Vec2.min(new Vec2(20, 10), new Vec2(15, 30));
        const expected = new Vec2(15, 10);

        expect(actual).toEqual(expected);
    });

    it('should calculate length', () => {
        const actual = new Vec2(10, 10).length;
        const expected = Math.sqrt(200);

        expect(actual).toBe(expected);
    });

    it('should calculate length squared', () => {
        const actual = new Vec2(10, 10).lengtSquared;
        const expected = 200;

        expect(actual).toBe(expected);
    });

    it('should calculate median', () => {
        const actual = Vec2.createMedian(new Vec2(10, 20), new Vec2(20, 20), new Vec2(60, 20));
        const expected = new Vec2(30, 20);

        expect(actual).toEqual(expected);
    });

    it('should calculate rotated vector correctly', () => {
        const source = new Vec2(40, 20);
        const center = new Vec2(20, 20);

        const rotation = Rotation.createFromRadian(Math.PI / 2);

        const actual = Vec2.createRotated(source, center, rotation);
        const expected = new Vec2(20, 40);

        expect(actual).toEqual(expected);
    });

    it('should calculate angles between vectors', () => {
        expect(Vec2.angleBetween(new Vec2(1, 0), new Vec2(1, 0))).toBe(0);
        expect(Vec2.angleBetween(new Vec2(1, 0), new Vec2(1, 1))).toBe(45);
        expect(Vec2.angleBetween(new Vec2(1, 0), new Vec2(0, 1))).toBe(90);
    });
});