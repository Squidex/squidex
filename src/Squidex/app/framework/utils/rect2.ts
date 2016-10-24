/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Rotation } from './rotation';
import { Vec2 }     from './vec2';

export class Rect2 {
    public static readonly ZERO = new Rect2(Vec2.ZERO, Vec2.ZERO);
    public static readonly EMPTY = new Rect2(Vec2.NEGATIVE_INFINITE, Vec2.NEGATIVE_INFINITE);
    public static readonly INFINITE = new Rect2(Vec2.POSITIVE_INFINITE, Vec2.POSITIVE_INFINITE);

    public get center(): Vec2 {
        return new Vec2(this.position.x + (0.5 * this.size.x), this.position.y + (0.5 * this.size.y));
    }

    public get area(): number {
        return this.size.x * this.size.y;
    }

    public get x(): number {
        return this.position.x;
    }

    public get y(): number {
        return this.position.y;
    }

    public get left(): number {
        return this.position.x;
    }

    public get top(): number {
        return this.position.y;
    }

    public get right(): number {
        return this.position.x + this.size.x;
    }

    public get bottom(): number {
        return this.position.y + this.size.y;
    }

    public get width(): number {
        return this.size.x;
    }

    public get height(): number {
        return this.size.y;
    }

    public get centerX(): number {
        return this.position.x + (0.5 * this.size.x);
    }

    public get centerY(): number {
        return this.position.y + (0.5 * this.size.y);
    }

    public get isEmpty(): boolean {
        return this.size.x < 0 || this.size.y < 0;
    }

    public get isInfinite(): boolean {
        return this.size.x === Number.POSITIVE_INFINITY || this.size.y === Number.POSITIVE_INFINITY;
    }

    constructor(
        public readonly position: Vec2,
        public readonly size: Vec2
    ) {
    }

    public static createFromVecs(vecs: Vec2[] | null): Rect2 {
        if (!vecs || vecs.length === 0) {
            return Rect2.EMPTY;
        }

        let minX = Number.MAX_VALUE;
        let minY = Number.MAX_VALUE;
        let maxX = Number.MIN_VALUE;
        let maxY = Number.MIN_VALUE;

        for (let v of vecs) {
            minX = Math.min(minX, v.x);
            minY = Math.min(minY, v.y);
            maxX = Math.max(maxX, v.x);
            maxY = Math.max(maxY, v.y);
        }

        return new Rect2(new Vec2(minX, minY), new Vec2(Math.max(0, maxX - minX), Math.max(0, maxY - minY)));
    }

    public static createFromRects(rects: Rect2[] | null): Rect2 {
        if (!rects || rects.length === 0) {
            return Rect2.EMPTY;
        }

        let minX = Number.MAX_VALUE;
        let minY = Number.MAX_VALUE;
        let maxX = Number.MIN_VALUE;
        let maxY = Number.MIN_VALUE;

        for (let r of rects) {
            minX = Math.min(minX, r.left);
            minY = Math.min(minY, r.top);
            maxX = Math.max(maxX, r.right);
            maxY = Math.max(maxY, r.bottom);
        }

        return new Rect2(new Vec2(minX, minY), new Vec2(Math.max(0, maxX - minX), Math.max(0, maxY - minY)));
    }

    public static createRotated(position: Vec2, size: Vec2, rotation: Rotation): Rect2 {
        const x = position.x;
        const y = position.y;
        const w = size.x;
        const h = size.y;

        if (Math.abs(rotation.sin) < Number.EPSILON) {
            return new Rect2(position, size);
        }

        const center = new Vec2(x + (w * 0.5), y + (h * 0.5));

        const lt = Vec2.createRotated(new Vec2(x + 0, y + 0), center, rotation);
        const rt = Vec2.createRotated(new Vec2(x + w, y + 0), center, rotation);
        const rb = Vec2.createRotated(new Vec2(x + w, y + h), center, rotation);
        const lb = Vec2.createRotated(new Vec2(x + 0, y + h), center, rotation);

        return Rect2.createFromVecs([lb, lt, rb, rt]);
    }

    public eq(r: Rect2): boolean {
        return this.position.eq(r.position) && this.size.eq(r.size);
    }

    public ne(r: Rect2): boolean {
        return this.position.ne(r.position) || this.size.ne(r.size);
    }

    public toString(): string {
        return `(x: ${this.x}, y: ${this.y}, w: ${this.width}, h: ${this.height})`;
    }

    public inflateV(v: Vec2): Rect2 {
        return this.inflate(v.x, v.y);
    }

    public inflate(w: number, h: number): Rect2 {
        return new Rect2(new Vec2(this.position.x - w, this.position.y - h), new Vec2(this.size.x + (2 * w), this.size.y + (2 * h)));
    }

    public deflateV(v: Vec2): Rect2 {
        return this.deflate(v.x, v.y);
    }

    public deflate(w: number, h: number): Rect2 {
        return new Rect2(new Vec2(this.position.x + w, this.position.y + h), new Vec2(Math.max(0, this.size.x - (2 * w)), Math.max(0, this.size.y - (2 * h))));
    }

    public containsRect(r: Rect2): boolean {
        return r.left >= this.left && r.right <= this.right && r.top >= this.top && r.bottom <= this.bottom;
    }

    public containsVec(v: Vec2): boolean {
        return v.x >= this.position.x && v.x - this.size.x <= this.position.x && v.y >= this.position.y && v.y - this.size.y <= this.position.y;
    }

    public intersectsWith(r: Rect2): boolean {
        return !this.isEmpty && !r.isEmpty && ((r.left <= this.right && r.right >= this.left && r.top <= this.bottom && r.bottom >= this.top) || this.isInfinite || r.isInfinite);
    }

    public intersect(r: Rect2): Rect2 {
        if (!this.intersectsWith(r)) {
            return Rect2.EMPTY;
        }

        const minX = Math.max(this.x, r.x);
        const minY = Math.max(this.y, r.y);

        const w = Math.min(this.position.x + this.size.x, r.position.x + r.size.x) - minX;
        const h = Math.min(this.position.y + this.size.y, r.position.y + r.size.y) - minY;

        return new Rect2(new Vec2(minX, minY), new Vec2(Math.max(w, 0.0), Math.max(h, 0.0)));
    }
}