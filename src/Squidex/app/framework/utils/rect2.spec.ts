/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    Rect2,
    Rotation,
    Vec2
} from './../';

describe('Rect2', () => {
    it('should provide values from constructor', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        expect(rect.x).toBe(10);
        expect(rect.y).toBe(20);
        expect(rect.top).toBe(20);
        expect(rect.left).toBe(10);
        expect(rect.right).toBe(60);
        expect(rect.width).toBe(50);
        expect(rect.height).toBe(30);
        expect(rect.bottom).toBe(50);
        expect(rect.centerX).toBe(35);
        expect(rect.centerY).toBe(35);

        expect(rect.size).toEqual(new Vec2(50, 30));
        expect(rect.center).toEqual(new Vec2(35, 35));
        expect(rect.position).toEqual(new Vec2(10, 20));

        expect(rect.area).toBe(1500);

        expect(rect.toString()).toBe('(x: 10, y: 20, w: 50, h: 30)');
    });

    it('should calculate isEmpty correctly', () => {
        expect(new Rect2(new Vec2(10, 20), new Vec2(50, 30)).isEmpty).toBeFalsy();
        expect(new Rect2(new Vec2(10, 20), new Vec2(-1, 30)).isEmpty).toBeTruthy();
        expect(new Rect2(new Vec2(10, 20), new Vec2(50, -9)).isEmpty).toBeTruthy();
    });

    it('should calculate isInfinite correctly', () => {
        expect(new Rect2(new Vec2(10, 20), new Vec2(50, 30)).isInfinite).toBeFalsy();
        expect(new Rect2(new Vec2(10, 20), new Vec2(Number.POSITIVE_INFINITY, 30)).isInfinite).toBeTruthy();
        expect(new Rect2(new Vec2(10, 20), new Vec2(50, Number.POSITIVE_INFINITY)).isInfinite).toBeTruthy();
    });

    it('should inflate correctly', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        const actual1 = rect.inflateV(new Vec2(10, 20));
        const actual2 = rect.inflate(10, 20);
        const expected = new Rect2(new Vec2(0, 0), new Vec2(70, 70));

        expect(actual1).toEqual(expected);
        expect(actual2).toEqual(expected);
    });

    it('should deflate correctly', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        const actual1 = rect.deflateV(new Vec2(25, 15));
        const actual2 = rect.deflate(25, 15);
        const expected = new Rect2(new Vec2(35, 35), new Vec2(0, 0));

        expect(actual1).toEqual(expected);
        expect(actual2).toEqual(expected);
    });

    it('should return true for intersection with infinite rect', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        expect(rect.intersectsWith(Rect2.INFINITE)).toBeTruthy();

        expect(Rect2.INFINITE.intersectsWith(rect)).toBeTruthy();
        expect(Rect2.INFINITE.intersectsWith(Rect2.INFINITE)).toBeTruthy();
    });

    it('should return false for intersection with empty rect', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        expect(rect.intersectsWith(Rect2.EMPTY)).toBeFalsy();

        expect(Rect2.EMPTY.intersectsWith(rect)).toBeFalsy();
        expect(Rect2.EMPTY.intersectsWith(Rect2.INFINITE)).toBeFalsy();
    });

    it('should return empty for no intersection', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));
        const outer = new Rect2(new Vec2(100, 20), new Vec2(50, 30));
        
        const actual = rect.intersect(outer);
        const expected = Rect2.EMPTY;

        expect(actual).toEqual(expected);
    });

    it('should return result for intersection', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));
        const inner = new Rect2(new Vec2(35, 35), new Vec2(100, 30));
        
        const actual = rect.intersect(inner);
        const expected = new Rect2(new Vec2(35, 35), new Vec2(25, 15));

        expect(actual).toEqual(expected);
    });

    it('should make correct contains by vector tests', () => {
        const rect = new Rect2(new Vec2(10, 20), new Vec2(50, 30));

        expect(rect.containsVec(rect.center)).toBeTruthy();
        expect(rect.containsVec(new Vec2(rect.left, rect.top))).toBeTruthy();

        expect(rect.containsVec(new Vec2(rect.centerX, 0))).toBeFalsy();
        expect(rect.containsVec(new Vec2(rect.centerX, 100))).toBeFalsy();
        expect(rect.containsVec(new Vec2(100, rect.centerY))).toBeFalsy();
        expect(rect.containsVec(new Vec2(-50, rect.centerY))).toBeFalsy();
    });

    it('should return true when rect contains other rect', () => {
        const rect = new Rect2(new Vec2(400, 400), new Vec2(400, 400));
        const other = new Rect2(new Vec2(500, 500), new Vec2(200, 200));

        expect(rect.containsRect(other)).toBeTruthy();
    });

    it('should return false when other rect is too top', () => {
        const rect = new Rect2(new Vec2(400, 400), new Vec2(400, 400));
        const other = new Rect2(new Vec2(300, 900), new Vec2(300, 100));

        expect(rect.containsRect(other)).toBeFalsy();
    });

    it('should return false when other rect is too bottom', () => {
        const rect = new Rect2(new Vec2(400, 400), new Vec2(400, 400));
        const other = new Rect2(new Vec2(300, 900), new Vec2(100, 300));

        expect(rect.containsRect(other)).toBeFalsy();
    });

    it('should return false when other rect is too left', () => {
        const rect = new Rect2(new Vec2(400, 400), new Vec2(400, 400));
        const other = new Rect2(new Vec2(200, 200), new Vec2(100, 300));

        expect(rect.containsRect(other)).toBeFalsy();
    });

    it('should return false when other right is too left', () => {
        const rect = new Rect2(new Vec2(400, 400), new Vec2(400, 400));
        const other = new Rect2(new Vec2(900, 200), new Vec2(100, 300));

        expect(rect.containsRect(other)).toBeFalsy();
    });

    it('should return empty when creating from null vectors', () => {
        const actual = Rect2.createFromVecs(null);
        const expected = Rect2.EMPTY;

        expect(actual).toEqual(expected);
    });

    it('should return empty when creating from null rects', () => {
        const actual = Rect2.createFromRects(null);
        const expected = Rect2.EMPTY;

        expect(actual).toEqual(expected);
    });

    it('should provide valid zero instance', () => {
        const actual = Rect2.ZERO;
        const expected = new Rect2(new Vec2(0, 0), new Vec2(0, 0));
        
        expect(actual).toEqual(expected);
    });

    it('should provide valid empty instance', () => {
        const actual = Rect2.EMPTY;
        const expected = new Rect2(Vec2.NEGATIVE_INFINITE, Vec2.NEGATIVE_INFINITE);

        expect(actual).toEqual(expected);
    });

    it('should provide valid infinite instance', () => {
        const actual = Rect2.INFINITE;
        const expected = new Rect2(Vec2.POSITIVE_INFINITE, Vec2.POSITIVE_INFINITE);

        expect(actual).toEqual(expected);
    });

    it('should create correct rect from vectors', () => {
        const actual =
            Rect2.createFromVecs([
                new Vec2(100, 100),
                new Vec2(500, 300),
                new Vec2(900, 800)]);
        const expected = new Rect2(new Vec2(100, 100), new Vec2(800, 700));

        expect(actual).toEqual(expected);
    });

    it('should create correct rect from rects', () => {
        const actual =
            Rect2.createFromRects([
                new Rect2(new Vec2(100, 100), new Vec2(100, 100)),
                new Rect2(new Vec2(500, 300), new Vec2(100, 100)),
                new Rect2(new Vec2(150, 150), new Vec2(750, 650))]);
        const expected = new Rect2(new Vec2(100, 100), new Vec2(800, 700));

        expect(actual).toEqual(expected);
    });

    it('should create rect from rotation', () => {
        const actual = Rect2.createRotated(new Vec2(400, 300), new Vec2(600, 400), Rotation.createFromRadian(Math.PI / 2));
        const expected = new Rect2(new Vec2(500, 200), new Vec2(400, 600));

        expect(actual).toEqual(expected);
    });

    it('should create rect from zero rotation', () => {
        const actual = Rect2.createRotated(new Vec2(400, 300), new Vec2(600, 400), Rotation.ZERO);
        const expected = new Rect2(new Vec2(400, 300), new Vec2(600, 400));

        expect(actual).toEqual(expected);
    });

    it('should make valid equal comparisons', () => {
        expect(new Rect2(new Vec2(10, 10), new Vec2(10, 10)).eq(new Rect2(new Vec2(10, 10), new Vec2(10, 10)))).toBeTruthy();
        expect(new Rect2(new Vec2(10, 10), new Vec2(10, 10)).eq(new Rect2(new Vec2(20, 20), new Vec2(20, 20)))).toBeFalsy();
    });

    it('should make valid not equal comparisons', () => {
        expect(new Rect2(new Vec2(10, 10), new Vec2(10, 10)).ne(new Rect2(new Vec2(10, 10), new Vec2(10, 10)))).toBeFalsy();
        expect(new Rect2(new Vec2(10, 10), new Vec2(10, 10)).ne(new Rect2(new Vec2(20, 20), new Vec2(20, 20)))).toBeTruthy();
    });
});